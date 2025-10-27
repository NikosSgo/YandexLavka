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

        builder.Property(pt => pt.CompletedAt)
            .HasColumnName("completed_at");

        // OwnsMany для PickingItem
        builder.OwnsMany(pt => pt.Items, item =>
        {
            item.ToTable("picking_items");
            item.WithOwner().HasForeignKey("picking_task_id");
            item.HasKey("PickingTaskId", "ProductId");

            item.Property(i => i.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            item.Property(i => i.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(200)
                .IsRequired();

            item.Property(i => i.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            item.Property(i => i.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            item.Property(i => i.StorageLocation)
                .HasColumnName("storage_location")
                .HasMaxLength(20)
                .IsRequired();

            item.Property(i => i.Barcode)
                .HasColumnName("barcode")
                .HasMaxLength(100);
        });

        builder.HasIndex(pt => pt.OrderId);
        builder.HasIndex(pt => pt.AssignedPicker);
        builder.HasIndex(pt => pt.Status);
        builder.HasIndex(pt => pt.Zone);
    }
}