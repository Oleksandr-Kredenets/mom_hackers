using TMS.Domain.Models;

namespace TMS.Domain.Interfaces;

public interface IDeliveryPointRepository
{
    Task<IReadOnlyList<DeliveryPointListItem>> GetDeliveryPointsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddDeliveryPointAsync(DeliveryPoint deliveryPoint, CancellationToken cancellationToken = default);

    Task<bool> TryDeleteDeliveryPointForUserAsync(
        Guid deliveryPointId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> TryUpdateDeliveryPointForUserAsync(
        Guid deliveryPointId,
        Guid userId,
        int? latitude,
        int? longitude,
        CancellationToken cancellationToken = default);
}
