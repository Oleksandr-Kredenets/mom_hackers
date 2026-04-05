using TMS.Domain.Enums;

namespace TMS.Domain.Models;

/// <summary>
/// One stop in a route plan (API input). Distinct from persisted <see cref="RoutePoint"/>.
/// </summary>
public sealed class RoutePlanPoint
{
    public int Sequence { get; init; }
    public RoutePointType Type { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

/// <summary>
/// Per-vehicle attributes for planning. Add optional properties here as OR-Tools / routing rules grow.
/// </summary>
public sealed class RouteVehicleSpecification
{
    public required string Id { get; init; }
    public double? MaxTravelMeters { get; init; }

    /// <summary>
    /// Maximum number of waypoint stops this vehicle may serve (unit demand per waypoint). Omit for no per-vehicle cap.
    /// </summary>
    public double? CapacityKg { get; init; }
}

/// <summary>
/// Request body for POST /api/route.
/// </summary>
public sealed class RoutePlanRequest
{
    public required List<RoutePlanPoint> Points { get; init; }
    public List<RouteVehicleSpecification> Vehicles { get; init; } = [];
}
