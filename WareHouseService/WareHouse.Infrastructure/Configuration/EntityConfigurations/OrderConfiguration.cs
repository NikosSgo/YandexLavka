using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Configuration.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<OrderAggregate>
{
    public void Configure(EntityTypeBuilder<OrderAggregate> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.OrderId);
        builder.Property(o => o.OrderId)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .HasColumnName("customer_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.PickingStartedAt)
            .HasColumnName("picking_started_at");

        builder.Property(o => o.PickingCompletedAt)
            .HasColumnName("picking_completed_at");

        // OwnsMany для OrderLine (Value Objects)
        builder.OwnsMany(o => o.Lines, line =>
        {
            line.ToTable("order_lines");
            line.WithOwner().HasForeignKey("order_id");
            line.HasKey("OrderId", "ProductId");

            line.Property(l => l.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            line.Property(l => l.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(200)
                .IsRequired();

            line.Property(l => l.Sku)
                .HasColumnName("sku")
                .HasMaxLength(50)
                .IsRequired();

            line.Property(l => l.QuantityOrdered)
                .HasColumnName("quantity_ordered")
                .IsRequired();

            line.Property(l => l.QuantityPicked)
                .HasColumnName("quantity_picked")
                .IsRequired();

            line.Property(l => l.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });

        builder.Ignore(o => o.DomainEvents);
    }
}