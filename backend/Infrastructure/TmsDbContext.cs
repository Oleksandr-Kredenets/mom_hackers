using Microsoft.EntityFrameworkCore;
using TMS.Domain.Models;

namespace TMS.Infrastructure;

public class TmsDbContext : DbContext
{
    public TmsDbContext(DbContextOptions<TmsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoutePoint>()
            .HasOne<TMS.Domain.Models.Route>()
            .WithMany()
            .HasForeignKey(p => p.RouteId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<DeliveryPoint> DeliveryPoints { get; set; } = null!;
    public DbSet<RoutePoint> RoutePoints { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
}
