using Microsoft.EntityFrameworkCore;
using TMS.Domain.Models;
using TMS.Domain.Interfaces;
using TMS.Infrastructure.Contexts;

namespace TMS.Infrastructure.Repositories;

public class DeliveryPointRepository : IDeliveryPointRepository
{
    // не імплеменовано UpdateDeliveryPointAsync
    private readonly TMSDbContext _context;

    public DeliveryPointRepository(TMSDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryPoint> GetDeliveryPointByIdAsync(Guid id)
    {
        var deliveryPoint = await _context.DeliveryPoints
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.Id == id)
                      ?? throw new InvalidOperationException($"Delivery point {id} not found.");

        return deliveryPoint;
    }

    public async Task<IEnumerable<DeliveryPoint>> GetAllDeliveryPointsAsync()
    {
        List<DeliveryPoint> deliveryPoints = await _context.DeliveryPoints
                      .AsNoTracking()
                      .ToListAsync();

        return deliveryPoints;
    }

    public async Task AddDeliveryPointAsync(DeliveryPoint deliveryPoint)
    {
        await _context.DeliveryPoints.AddAsync(deliveryPoint);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDeliveryPointAsync(Guid id)
    {
        var deliveryPoint = await GetDeliveryPointByIdAsync(id);
        if (deliveryPoint != null)
        {
            _context.DeliveryPoints.Remove(deliveryPoint);
            await _context.SaveChangesAsync();
        }
    }
}