using Microsoft.EntityFrameworkCore;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure;

namespace TMS.Infrastructure.Repositories;

public class DeliveryPointRepository : IDeliveryPointRepository
{
    private readonly TmsDbContext _context;

    public DeliveryPointRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DeliveryPointListItem>> GetDeliveryPointsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.DeliveryPoints.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(p => new DeliveryPointListItem
            {
                Id = p.Id,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
            })
            .ToList();
    }

    public async Task AddDeliveryPointAsync(DeliveryPoint deliveryPoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deliveryPoint);
        await _context.DeliveryPoints.AddAsync(deliveryPoint, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> TryDeleteDeliveryPointForUserAsync(
        Guid deliveryPointId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var point = await _context.DeliveryPoints.FirstOrDefaultAsync(
                p => p.Id == deliveryPointId && p.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (point is null)
            return false;

        _context.DeliveryPoints.Remove(point);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> TryUpdateDeliveryPointForUserAsync(
        Guid deliveryPointId,
        Guid userId,
        int? latitude,
        int? longitude,
        CancellationToken cancellationToken = default)
    {
        var point = await _context.DeliveryPoints.FirstOrDefaultAsync(
                p => p.Id == deliveryPointId && p.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (point is null)
            return false;

        if (latitude.HasValue)
            point.Latitude = latitude.Value;
        if (longitude.HasValue)
            point.Longitude = longitude.Value;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
