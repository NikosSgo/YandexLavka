using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Configuration.EntityConfigurations;

public class PickingItemConfiguration : IEntityTypeConfiguration<PickingItem>
{
    public void Configure(EntityTypeBuilder<PickingItem> builder)
    {
        builder.ToTable("picking_items");

        builder.HasKey(pi => new { pi.PickingTaskId, pi.ProductId });

        builder.Property(pi => pi.PickingTaskId)
            .HasColumnName("picking_task_id")
            .IsRequired();

        builder.Property(pi => pi.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(pi => pi.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(pi => pi.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pi => pi.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(pi => pi.QuantityPicked)
            .HasColumnName("quantity_picked")
            .IsRequired();

        builder.Property(pi => pi.StorageLocation)
            .HasColumnName("storage_location")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pi => pi.Barcode)
            .HasColumnName("barcode")
            .HasMaxLength(100);

        builder.Property(pi => pi.IsPicked)
            .HasColumnName("is_picked")
            .IsRequired();

        // Связь с PickingTask
        builder.HasOne(pi => pi.PickingTask)
            .WithMany(pt => pt.Items)
            .HasForeignKey(pi => pi.PickingTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}