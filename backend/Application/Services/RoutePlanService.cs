using System.Text.Json;
using System.Text.Json.Nodes;
using TMS.Application.Interfaces;
using TMS.Application.Models;
using TMS.Application.Models.GeoJson;
using TMS.Application.Routing;
using TMS.Domain.Enums;
using TMS.Domain.Models;

namespace TMS.Application.Services;

public class RoutePlanService : IRoutePlanService
{
    private static readonly JsonSerializerOptions SpecJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IValhallaClient _valhalla;

    public RoutePlanService(IValhallaClient valhalla)
    {
        _valhalla = valhalla;
    }

    public async Task<GeoJsonFeatureCollection> BuildGeoJsonAsync(
        RoutePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Points.Count < 2)
            throw new ArgumentException("At least two points are required.", nameof(request));
        if (request.Vehicles.Count < 1)
            throw new ArgumentException("At least one vehicle is required.", nameof(request));

        var ordered = request.Points.OrderBy(p => p.Sequence).ToList();
        ValidatePointSequence(ordered);

        var start = ordered[0];
        var end = ordered[^1];
        var waypoints = ordered.Skip(1).Take(ordered.Count - 2).ToList();
        foreach (var w in waypoints)
        {
            if (w.Type != RoutePointType.Waypoint)
                throw new ArgumentException("All points between Start and End must be Waypoints.");
        }

        var nodes = new List<GeoCoordinate>
        {
            new(start.Latitude, start.Longitude),
        };
        foreach (var w in waypoints)
            nodes.Add(new GeoCoordinate(w.Latitude, w.Longitude));
        nodes.Add(new(end.Latitude, end.Longitude));

        var matrices = await _valhalla.GetMatricesAsync(nodes, cancellationToken).ConfigureAwait(false);

        var depotIndex = 0;
        var endIndex = nodes.Count - 1;
        var routes = OrToolsVehicleRouteOptimizer.Solve(
            matrices.DurationSeconds,
            matrices.DistanceMeters,
            request.Vehicles,
            depotIndex,
            endIndex,
            waypointNodeCount: waypoints.Count);

        var features = new List<GeoJsonFeature>();
        for (var v = 0; v < routes.Count; v++)
        {
            var vehicle = request.Vehicles[v];
            var nodePath = routes[v];
            if (nodePath.Count < 2)
                continue;

            var stops = new List<GeoCoordinate>();
            foreach (var node in nodePath)
            {
                var coord = nodes[node];
                if (stops.Count > 0
                    && stops[^1].Latitude == coord.Latitude
                    && stops[^1].Longitude == coord.Longitude)
                    continue;
                stops.Add(coord);
            }

            if (stops.Count < 2)
                continue;

            var line = await _valhalla
                .GetRouteShapeLonLatAsync(stops, cancellationToken)
                .ConfigureAwait(false);

            features.Add(new GeoJsonFeature
            {
                Geometry = new GeoJsonLineStringGeometry { Coordinates = line },
                Properties = new Dictionary<string, JsonNode>
                {
                    ["kind"] = "vehicle_route",
                    ["vehicleId"] = vehicle.Id,
                    ["vehicleIndex"] = v,
                    ["spec"] = JsonSerializer.SerializeToNode(vehicle, SpecJsonOptions)
                        ?? JsonValue.Create(vehicle.Id),
                },
            });
        }

        var depot = new GeoCoordinate(start.Latitude, start.Longitude);
        foreach (var vehicle in request.Vehicles)
        {
            features.Add(new GeoJsonFeature
            {
                Geometry = new GeoJsonPointGeometry
                {
                    Coordinates = [depot.Longitude, depot.Latitude],
                },
                Properties = new Dictionary<string, JsonNode>
                {
                    ["kind"] = "vehicle",
                    ["spec"] = JsonSerializer.SerializeToNode(vehicle, SpecJsonOptions)
                        ?? JsonValue.Create(vehicle.Id),
                },
            });
        }

        return new GeoJsonFeatureCollection { Features = features };
    }

    private static void ValidatePointSequence(IReadOnlyList<RoutePlanPoint> ordered)
    {
        var starts = ordered.Count(p => p.Type == RoutePointType.Start);
        var ends = ordered.Count(p => p.Type == RoutePointType.End);
        if (starts != 1 || ends != 1)
            throw new ArgumentException("Exactly one Start and one End point are required.");

        if (ordered[0].Type != RoutePointType.Start || ordered[^1].Type != RoutePointType.End)
            throw new ArgumentException("The first point must be Start and the last must be End after ordering by sequence.");
    }
}
