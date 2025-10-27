using Microsoft.EntityFrameworkCore;
using WareHouse.Domain.Entities;
using WareHouse.Infrastructure.Configuration.EntityConfigurations;

namespace WareHouse.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<OrderAggregate> Orders { get; set; }
    public DbSet<PickingTask> PickingTasks { get; set; }
    public DbSet<StorageUnit> StorageUnits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new PickingTaskConfiguration());
        modelBuilder.ApplyConfiguration(new StorageUnitConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Доменные события будут обработаны через DomainEventService
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}