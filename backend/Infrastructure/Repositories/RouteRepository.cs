using Microsoft.EntityFrameworkCore;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure;
using RouteEntity = TMS.Domain.Models.Route;

namespace TMS.Infrastructure.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly TmsDbContext _context;

    public RouteRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task AddRouteAsync(
        RouteEntity route,
        IEnumerable<RoutePoint> points,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(points);
        await _context.Routes.AddAsync(route, cancellationToken).ConfigureAwait(false);
        await _context.RoutePoints.AddRangeAsync(points, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RouteListItem>> GetActiveRoutesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var routes = await _context.Routes.AsNoTracking()
            .Where(r => r.UserId == userId && r.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (routes.Count == 0)
            return [];

        var ids = routes.Select(r => r.Id).ToList();
        var counts = await _context.RoutePoints.AsNoTracking()
            .Where(p => ids.Contains(p.RouteId))
            .GroupBy(p => p.RouteId)
            .Select(g => new { RouteId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RouteId, x => x.Count, cancellationToken)
            .ConfigureAwait(false);

        return routes
            .Select(r => new RouteListItem(r.Id, r.IsActive, counts.GetValueOrDefault(r.Id, 0)))
            .ToList();
    }

    public async Task<RouteDetailResponse?> GetRouteDetailForUserAsync(
        Guid routeId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var route = await _context.Routes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == routeId && r.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (route is null)
            return null;

        var points = await _context.RoutePoints.AsNoTracking()
            .Where(p => p.RouteId == routeId)
            .OrderBy(p => p.Sequence)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new RouteDetailResponse
        {
            Id = route.Id,
            IsActive = route.IsActive,
            Points = points
                .Select(p => new RoutePointResponse
                {
                    Sequence = p.Sequence,
                    Type = p.Type,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                })
                .ToList(),
        };
    }

    public async Task<bool> TryDeleteRouteForUserAsync(
        Guid routeId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(
                r => r.Id == routeId && r.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (route is null)
            return false;

        var points = await _context.RoutePoints.Where(p => p.RouteId == routeId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        _context.RoutePoints.RemoveRange(points);
        _context.Routes.Remove(route);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> TryUpdateRouteForUserAsync(
        Guid routeId,
        Guid userId,
        bool? isActive,
        IReadOnlyList<RoutePoint>? replacementPoints,
        CancellationToken cancellationToken = default)
    {
        var route = await _context.Routes.FirstOrDefaultAsync(
                r => r.Id == routeId && r.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (route is null)
            return false;

        if (isActive.HasValue)
            route.IsActive = isActive.Value;

        if (replacementPoints is not null)
        {
            var oldPoints = await _context.RoutePoints.Where(p => p.RouteId == routeId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            _context.RoutePoints.RemoveRange(oldPoints);
            await _context.RoutePoints.AddRangeAsync(replacementPoints, cancellationToken)
                .ConfigureAwait(false);
        }

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
