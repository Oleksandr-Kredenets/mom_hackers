using Microsoft.EntityFrameworkCore;
using TMS.Domain.Models;

namespace TMS.Infrastructure.Contexts;

public class TMSDbContext : DbContext
{
    public TMSDbContext(DbContextOptions<TMSDbContext> options) : base(options)
    {
    }

    public DbSet<DeliveryPoint> DeliveryPoints { get; set; } = null!;
    public DbSet<RoutePoint> RoutePoints { get; set; } = null!;
    public DbSet<Domain.Models.Route> Routes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
}
