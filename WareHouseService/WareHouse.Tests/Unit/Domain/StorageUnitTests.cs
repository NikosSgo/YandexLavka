using FluentAssertions;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Exceptions;
using Xunit;

namespace WareHouse.Tests.Unit.Domain;

public class StorageUnitTests
{
    [Fact]
    public void Reserve_WhenSufficientQuantity_ShouldReserveQuantity()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 10);

        // Act
        storageUnit.Reserve(5);

        // Assert
        storageUnit.ReservedQuantity.Should().Be(5);
        storageUnit.AvailableQuantity.Should().Be(5);
    }

    [Fact]
    public void Reserve_WhenInsufficientQuantity_ShouldThrowException()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 5);

        // Act & Assert
        storageUnit.Invoking(su => su.Reserve(10))
            .Should().Throw<DomainException>()
            .WithMessage("*insufficient available quantity*");
    }

    [Fact]
    public void Pick_WhenQuantityIsReserved_ShouldReduceQuantity()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 10);
        storageUnit.Reserve(5);

        // Act
        storageUnit.Pick(5);

        // Assert
        storageUnit.Quantity.Should().Be(5);
        storageUnit.ReservedQuantity.Should().Be(0);
        storageUnit.AvailableQuantity.Should().Be(5);
    }

    [Fact]
    public void Pick_WhenQuantityExceedsReserved_ShouldThrowException()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 10);
        storageUnit.Reserve(5);

        // Act & Assert
        storageUnit.Invoking(su => su.Pick(6))
            .Should().Throw<DomainException>()
            .WithMessage("*cannot pick more than reserved*");
    }

    [Fact]
    public void Restock_ShouldIncreaseQuantity()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 10);

        // Act
        storageUnit.Restock(5);

        // Assert
        storageUnit.Quantity.Should().Be(15);
        storageUnit.LastRestocked.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void IsLowStock_WhenQuantityBelowThreshold_ShouldReturnTrue()
    {
        // Arrange
        var storageUnit = CreateTestStorageUnit(quantity: 5, capacity: 100);

        // Act & Assert
        storageUnit.IsLowStock.Should().BeTrue();
    }

    private static StorageUnit CreateTestStorageUnit(int quantity = 10, int capacity = 100)
    {
        return new StorageUnit(
            Guid.NewGuid(),
            "Test Product",
            "SKU-TEST",
            quantity,
            "A-01-01",
            "Zone-A"
        );
    }
}