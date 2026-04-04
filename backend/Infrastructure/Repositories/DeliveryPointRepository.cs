using Microsoft.EntityFrameworkCore;
using TMS.Domain.Models;

namespace TMS.Infrastructure.Repositories;
public class DeliveryPointRepository : IDeliveryPointRepository
{
    private readonly TMSDbContext _context;

    public DeliveryPointRepository(TMSDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryPoint?> GetByIdAsync(Guid id)
    {
        DeliveryPoint? deliveryPoint = await _context.DeliveryPoints
                      .AsNoTracking()
                      .FindOrDefaultAsync(p => p.Id == id);

        return deliveryPoint;
    }

    public async Task<IEnumerable<DeliveryPoint>> GetAllAsync()
    {
        List<DeliveryPoint> deliveryPoints = await _context.DeliveryPoints
                      .AsNoTracking()
                      .ToListAsync();

        return deliveryPoints;
    }

    public async Task AddAsync(DeliveryPoint deliveryPoint)
    {
        _context.DeliveryPoints.AddAsync(deliveryPoint);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var deliveryPoint = await GetByIdAsync(id);
        if (deliveryPoint != null)
        {
            _context.DeliveryPoints.Remove(deliveryPoint);
            await _context.SaveChangesAsync();
        }
    }
}