using TMS.Domain.Models;
namespace TMS.Domain.Interfaces;

public interface IRouteRepository
{
    Task AddRouteAsync(Models.Route route, IEnumerable<Models.RoutePoint> points, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Models.RouteListItem>> GetActiveRoutesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Models.RouteDetailResponse?> GetRouteDetailForUserAsync(
        Guid routeId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> TryDeleteRouteForUserAsync(Guid routeId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// When <paramref name="replacementPoints"/> is non-null, replaces all points. Updates <see cref="Models.Route.IsActive"/> when <paramref name="isActive"/> is set.
    /// </summary>
    Task<bool> TryUpdateRouteForUserAsync(
        Guid routeId,
        Guid userId,
        bool? isActive,
        IReadOnlyList<Models.RoutePoint>? replacementPoints,
        CancellationToken cancellationToken = default);

    public Task<List<Dictionary<Route, List<RoutePoint>>>> GetAllRoutesAsync();
    public Task AddRouteAsync(Route route, List<RoutePoint> points);
    public Task<bool> DeleteRouteByIdAsync(Guid id);
    Task DeleteAllRoutesAsync();
}