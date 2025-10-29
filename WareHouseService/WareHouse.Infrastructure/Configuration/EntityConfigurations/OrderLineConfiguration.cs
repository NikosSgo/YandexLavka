using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Configuration.EntityConfigurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(ol => new { ol.OrderId, ol.ProductId });

        builder.Property(ol => ol.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(ol => ol.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(ol => ol.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ol => ol.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ol => ol.QuantityOrdered)
            .HasColumnName("quantity_ordered")
            .IsRequired();

        builder.Property(ol => ol.QuantityPicked)
            .HasColumnName("quantity_picked")
            .IsRequired();

        builder.Property(ol => ol.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Связь с Order
        builder.HasOne(ol => ol.Order)
            .WithMany(o => o.Lines)
            .HasForeignKey(ol => ol.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}