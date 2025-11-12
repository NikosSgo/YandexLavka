namespace UserService.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Value Objects - Phone
        builder.Property(u => u.Phone)
            .HasConversion(
                phone => phone.Value,
                value => new Domain.ValueObjects.Phone(value))
            .IsRequired()
            .HasMaxLength(20);

        // Value Objects - Email
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => new Domain.ValueObjects.Email(value))
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        // Настройка коллекции адресов
        // EF Core работает с публичным свойством Addresses (IReadOnlyCollection)
        // и использует приватное поле _addresses для хранения
        builder.HasMany(u => u.Addresses)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Настраиваем доступ к навигационному свойству через приватное поле
        var addressesNavigation = builder.Metadata.FindNavigation(nameof(User.Addresses));
        if (addressesNavigation != null)
        {
            addressesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        // Индексы для быстрого поиска
        // Индексы создаются на колонках после конвертации value objects в строки
        builder.HasIndex(u => u.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Users_Phone");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}

