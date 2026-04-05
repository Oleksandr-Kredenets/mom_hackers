using System.Text.Json;
using System.Text.Json.Nodes;
using TMS.Application.Interfaces;
using TMS.Application.Models;
using TMS.Application.Models.GeoJson;
using TMS.Application.Routing;
using TMS.Application.Validation;
using TMS.Domain.Enums;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using RouteEntity = TMS.Domain.Models.Route;

namespace TMS.Application.Services;

public class RoutePlanService : IRoutePlanService
{
    private static readonly JsonSerializerOptions SpecJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly List<RouteVehicleSpecification> DefaultVehicles =
    [
        new RouteVehicleSpecification { Id = "default" },
    ];

    private readonly IValhallaClient _valhalla;
    private readonly IRouteRepository _routeRepository;

    public RoutePlanService(IValhallaClient valhalla, IRouteRepository routeRepository)
    {
        _valhalla = valhalla;
        _routeRepository = routeRepository;
    }

    public async Task<GeoJsonFeatureCollection> BuildGeoJsonAsync(
        RoutePlanRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vehicles = request.Vehicles.Count > 0 ? request.Vehicles : DefaultVehicles;

        var ordered = RoutePlanPointSequenceValidator.OrderAndValidate(request.Points);

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
            vehicles,
            depotIndex,
            endIndex,
            waypointNodeCount: waypoints.Count);

        var features = new List<GeoJsonFeature>();
        for (var v = 0; v < routes.Count; v++)
        {
            var vehicle = vehicles[v];
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

            var routeEntity = new RouteEntity { UserId = userId };
            var persistPoints = BuildPersistedRoutePoints(routeEntity.Id, stops);
            await _routeRepository.AddRouteAsync(routeEntity, persistPoints, cancellationToken)
                .ConfigureAwait(false);
        }

        var depot = new GeoCoordinate(start.Latitude, start.Longitude);
        foreach (var vehicle in vehicles)
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

    private static List<RoutePoint> BuildPersistedRoutePoints(Guid routeId, IReadOnlyList<GeoCoordinate> stops)
    {
        var list = new List<RoutePoint>(stops.Count);
        for (var i = 0; i < stops.Count; i++)
        {
            var type = i == 0
                ? RoutePointType.Start
                : i == stops.Count - 1
                    ? RoutePointType.End
                    : RoutePointType.Waypoint;
            var s = stops[i];
            list.Add(new RoutePoint
            {
                Id = Guid.NewGuid(),
                RouteId = routeId,
                Sequence = i,
                Type = type,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
            });
        }

        return list;
    }
}
