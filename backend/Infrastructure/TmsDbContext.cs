using Microsoft.EntityFrameworkCore;
using TMS.Domain.Models;

namespace TMS.Infrastructure.Contexts;

public class TMSDbContext : DbContext
{
    public TMSDbContext(DbContextOptions<TMSDbContext> options) : base(options)
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
    public DbSet<Domain.Models.Route> Routes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
}
