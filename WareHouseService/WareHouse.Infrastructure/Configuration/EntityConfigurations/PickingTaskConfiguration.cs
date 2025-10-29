using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Configuration.EntityConfigurations;

public class PickingTaskConfiguration : IEntityTypeConfiguration<PickingTask>
{
    public void Configure(EntityTypeBuilder<PickingTask> builder)
    {
        builder.ToTable("picking_tasks");

        builder.HasKey(pt => pt.TaskId);
        builder.Property(pt => pt.TaskId)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(pt => pt.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(pt => pt.AssignedPicker)
            .HasColumnName("assigned_picker")
            .HasMaxLength(50);

        builder.Property(pt => pt.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(pt => pt.Zone)
            .HasColumnName("zone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pt => pt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(pt => pt.StartedAt)
            .HasColumnName("started_at");

        builder.Property(pt => pt.CompletedAt)
            .HasColumnName("completed_at");

        // Явно игнорируем вычисляемые свойства
        builder.Ignore(pt => pt.AllItemsPicked);
        builder.Ignore(pt => pt.Progress);
        builder.Ignore(pt => pt.TotalItems);
        builder.Ignore(pt => pt.PickedItemsCount);

        // Связь с PickingItems
        builder.HasMany(pt => pt.Items)
            .WithOne(pi => pi.PickingTask)
            .HasForeignKey(pi => pi.PickingTaskId);

        builder.Ignore(pt => pt.DomainEvents);

        builder.HasIndex(pt => pt.OrderId);
        builder.HasIndex(pt => pt.AssignedPicker);
        builder.HasIndex(pt => pt.Status);
        builder.HasIndex(pt => pt.Zone);
    }
}