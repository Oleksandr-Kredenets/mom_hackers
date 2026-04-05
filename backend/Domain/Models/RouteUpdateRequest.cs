namespace TMS.Domain.Models;

public sealed class RouteUpdateRequest
{
    public bool? IsActive { get; init; }
    public List<RoutePlanPoint>? Points { get; init; }
}
