using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Configuration.EntityConfigurations;

public class StorageUnitConfiguration : IEntityTypeConfiguration<StorageUnit>
{
    public void Configure(EntityTypeBuilder<StorageUnit> builder)
    {
        builder.ToTable("storage_units");

        builder.HasKey(su => su.UnitId);
        builder.Property(su => su.UnitId)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(su => su.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(su => su.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(su => su.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(su => su.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(su => su.ReservedQuantity)
            .HasColumnName("reserved_quantity")
            .IsRequired();

        builder.Property(su => su.Location)
            .HasColumnName("location")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(su => su.Zone)
            .HasColumnName("zone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(su => su.LastRestocked)
            .HasColumnName("last_restocked")
            .IsRequired();

        builder.HasIndex(su => su.ProductId);
        builder.HasIndex(su => su.Location);
        builder.HasIndex(su => su.Zone);
        builder.HasIndex(su => new { su.ProductId, su.Location }).IsUnique();
    }
}