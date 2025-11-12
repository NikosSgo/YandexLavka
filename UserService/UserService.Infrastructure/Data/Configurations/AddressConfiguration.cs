namespace UserService.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever()
            .IsRequired();

        // Внешний ключ на User (хранится как тень)
        builder.Property<Guid>("UserId")
            .IsRequired();

        builder.HasIndex("UserId")
            .HasDatabaseName("IX_Addresses_UserId");

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ZipCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Description)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(a => a.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired(false);

        // Индекс для быстрого поиска основного адреса
        // В PostgreSQL синтаксис фильтрации отличается
        builder.HasIndex("UserId", "IsPrimary")
            .HasDatabaseName("IX_Addresses_UserId_IsPrimary")
            .HasFilter("\"IsPrimary\" = true");
    }
}

