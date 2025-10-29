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

        builder.Property(o => o.PackingCompletedAt)
            .HasColumnName("packing_completed_at");

        // Явно игнорируем вычисляемые свойства
        builder.Ignore(o => o.AllItemsPicked);
        builder.Ignore(o => o.TotalAmount);
        builder.Ignore(o => o.TotalItems);
        builder.Ignore(o => o.PickedItems);

        // Связь с OrderLines
        builder.HasMany(o => o.Lines)
            .WithOne(ol => ol.Order)
            .HasForeignKey(ol => ol.OrderId);

        builder.Ignore(o => o.DomainEvents);
    }
}