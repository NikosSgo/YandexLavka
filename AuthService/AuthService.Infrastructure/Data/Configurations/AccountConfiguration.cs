using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        // Конвертация Email value object в строку
        builder.Property(a => a.Email)
            .HasConversion(
                v => v.Value,
                v => new Email(v))
            .IsRequired()
            .HasMaxLength(256);

        // Конвертация PasswordHash value object в строку
        builder.Property(a => a.PasswordHash)
            .HasConversion(
                v => v.Value,
                v => new PasswordHash(v))
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.Property(a => a.LastLoginAt)
            .IsRequired(false);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired(false);

        // Уникальный индекс для Email
        builder.HasIndex(a => a.Email)
            .IsUnique();
    }
}

