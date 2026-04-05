using TMS.Domain.Models;

namespace TMS.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<IReadOnlyList<WarehouseListItem>> GetWarehousesForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddWarehouseAsync(Warehouse warehouse, CancellationToken cancellationToken = default);

    Task<bool> TryDeleteWarehouseForUserAsync(
        Guid warehouseId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> TryUpdateWarehouseForUserAsync(
        Guid warehouseId,
        Guid userId,
        int? latitude,
        int? longitude,
        CancellationToken cancellationToken = default);
}
