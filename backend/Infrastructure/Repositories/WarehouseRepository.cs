using Microsoft.EntityFrameworkCore;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure.Contexts;

namespace TMS.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly TMSDbContext _context;

    public WarehouseRepository(TMSDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WarehouseListItem>> GetWarehousesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Warehouses.AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(w => new WarehouseListItem
            {
                Id = w.Id,
                Latitude = w.Latitude,
                Longitude = w.Longitude,
            })
            .ToList();
    }

    public async Task AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(warehouse);
        await _context.Warehouses.AddAsync(warehouse, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> TryDeleteWarehouseForUserAsync(
        Guid warehouseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(
                w => w.Id == warehouseId && w.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (warehouse is null)
            return false;

        _context.Warehouses.Remove(warehouse);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> TryUpdateWarehouseForUserAsync(
        Guid warehouseId,
        Guid userId,
        int? latitude,
        int? longitude,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(
                w => w.Id == warehouseId && w.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (warehouse is null)
            return false;

        if (latitude.HasValue)
            warehouse.Latitude = latitude.Value;
        if (longitude.HasValue)
            warehouse.Longitude = longitude.Value;

        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
