namespace TMS.Domain.Interfaces;

using TMS.Domain.Models;

public interface IDeliveryPointRepository
{
    Task<DeliveryPoint> GetDeliveryPointByIdAsync(Guid id);
    Task<IEnumerable<DeliveryPoint>> GetAllDeliveryPointsAsync();
    Task AddDeliveryPointAsync(DeliveryPoint deliveryPoint);
    // Task UpdateDeliveryPointAsync(DeliveryPoint deliveryPoint);
    Task DeleteDeliveryPointAsync(Guid id);
}