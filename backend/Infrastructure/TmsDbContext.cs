using Microsogt.EntityFrameworkCore;
using DeliveryPoint.Domain.Models;

namespace TMS.Infrastructure.Contexts
{
    public class TmsDbContext : DbContext
    {
        public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options)
        {
        }

        public DbSet<DeliveryPoint> DeliveryPoints { get; set; } = null;
        public DbSet<RoutePoint> RoutePoints { get; set; } = null;
        public DbSet<Route> Routes { get; set; } = null;
    }
}