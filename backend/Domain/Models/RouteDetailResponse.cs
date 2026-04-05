using TMS.Domain.Enums;

namespace TMS.Domain.Models;

public sealed class RouteDetailResponse
{
    public required Guid Id { get; init; }
    public bool IsActive { get; init; }
    public required IReadOnlyList<RoutePointResponse> Points { get; init; }
}

public sealed class RoutePointResponse
{
    public int Sequence { get; init; }
    public RoutePointType Type { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
