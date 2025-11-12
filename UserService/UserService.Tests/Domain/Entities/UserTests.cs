using FluentAssertions;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.ValueObjects;
using Xunit;

namespace UserService.Tests.Unit.Domain.Entities;

public class UserTests
{
    private readonly Phone _validPhone;
    private readonly Email _validEmail;

    public UserTests()
    {
        _validPhone = new Phone("+79991234567");
        _validEmail = new Email("test@example.com");
    }

    private User CreateTestUser(string firstName = "John", string lastName = "Doe")
    {
        return new User(firstName, lastName, _validPhone, _validEmail);
    }

    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Act
        var user = CreateTestUser();

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Phone.Should().Be(_validPhone);
        user.Email.Should().Be(_validEmail);
        user.Addresses.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Doe")]
    [InlineData("John", "")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(
        string firstName,
        string lastName
    )
    {
        // Act & Assert
        Action act = () => new User(firstName, lastName, _validPhone, _validEmail);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangePhone_WithValidPhone_ShouldUpdatePhone()
    {
        // Arrange
        var user = CreateTestUser();
        var newPhone = new Phone("+79998765432");

        // Act
        user.ChangePhone(newPhone);

        // Assert
        user.Phone.Should().Be(newPhone);
    }

    [Fact]
    public void ChangeEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = CreateTestUser();
        var newEmail = new Email("new@example.com");

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdateName_WithValidNames_ShouldUpdateNames()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        user.UpdateName("Jane", "Smith");

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void AddAddress_WhenFirstAddress_ShouldAddAndReturnAddress()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var address = user.AddAddress("Lenina", "Moscow", "State", "Country", "123456", "Home");

        // Assert
        user.Addresses.Should().ContainSingle();
        address.Street.Should().Be("Lenina");
        address.City.Should().Be("Moscow");
    }

    [Fact]
    public void AddAddress_WhenMultipleAddresses_ShouldAllowAddingUpToMax()
    {
        // Arrange
        var user = CreateTestUser();

        // Act - Add 5 addresses (max limit)
        for (int i = 0; i < 5; i++)
        {
            user.AddAddress($"Street{i}", $"City{i}", "State", "Country", "12345", $"Address{i}");
        }

        // Assert
        user.GetAddressesCount().Should().Be(5);
        user.CanAddMoreAddresses().Should().BeFalse();
    }

    [Fact]
    public void AddAddress_WhenMaxAddressesReached_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = CreateTestUser();

        // Add maximum allowed addresses
        for (int i = 0; i < 5; i++)
        {
            user.AddAddress($"Street{i}", $"City{i}", "State", "Country", "12345", $"Address{i}");
        }

        // Act & Assert
        Action act = () =>
            user.AddAddress("ExtraStreet", "ExtraCity", "State", "Country", "12345", "Extra");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveAddress_WithExistingAddress_ShouldRemoveAddress()
    {
        // Arrange
        var user = CreateTestUser();
        var address = user.AddAddress("Lenina", "Moscow", "State", "Country", "123456", "Home");
        var addressId = address.Id;

        // Act
        user.RemoveAddress(addressId);

        // Assert
        user.Addresses.Should().BeEmpty();
        user.GetAddressesCount().Should().Be(0);
    }

    [Fact]
    public void RemoveAddress_WithNonExistingAddress_ShouldDoNothing()
    {
        // Arrange
        var user = CreateTestUser();
        var nonExistingId = Guid.NewGuid();

        // Act
        user.RemoveAddress(nonExistingId);

        // Assert
        user.Addresses.Should().BeEmpty(); // No exception thrown
    }

    [Fact]
    public void SetPrimaryAddress_WithExistingAddress_ShouldUpdatePrimaryFlag()
    {
        // Arrange
        var user = CreateTestUser();
        var address1 = user.AddAddress("Street1", "City1", "State1", "Country1", "111111", "Home");
        var address2 = user.AddAddress("Street2", "City2", "State2", "Country2", "222222", "Work");

        // Act
        user.SetPrimaryAddress(address2.Id);

        // Assert
        user.GetPrimaryAddress().Should().Be(address2);
    }

    [Fact]
    public void SetPrimaryAddress_WithNonExistingAddress_ShouldDoNothing()
    {
        // Arrange
        var user = CreateTestUser();
        var address = user.AddAddress("Street1", "City1", "State1", "Country1", "111111", "Home");
        var originalPrimary = user.GetPrimaryAddress();
        var nonExistingId = Guid.NewGuid();

        // Act
        user.SetPrimaryAddress(nonExistingId);

        // Assert
        // Проверяем, что primary адрес не изменился
        user.GetPrimaryAddress().Should().Be(originalPrimary);
    }

    [Fact]
    public void GetPrimaryAddress_WhenNoAddresses_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var primaryAddress = user.GetPrimaryAddress();

        // Assert
        primaryAddress.Should().BeNull();
    }

    [Fact]
    public void GetPrimaryAddress_WhenAddressesExistButNoPrimary_ShouldReturnNull()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddAddress("Street1", "City1", "State1", "Country1", "111111", "Home", false);
        user.AddAddress("Street2", "City2", "State2", "Country2", "222222", "Work", false);

        // Act
        var primaryAddress = user.GetPrimaryAddress();

        // Assert
        primaryAddress.Should().BeNull();
    }

    [Fact]
    public void UpdateAddress_WithExistingAddress_ShouldUpdateAddressData()
    {
        // Arrange
        var user = CreateTestUser();
        var address = user.AddAddress(
            "OldStreet",
            "OldCity",
            "State",
            "Country",
            "111111",
            "OldDesc"
        );

        // Act
        user.UpdateAddress(
            address.Id,
            "NewStreet",
            "NewCity",
            "State",
            "Country",
            "222222",
            "NewDesc"
        );

        // Assert
        address.Street.Should().Be("NewStreet");
        address.City.Should().Be("NewCity");
        address.ZipCode.Should().Be("222222");
        address.Description.Should().Be("NewDesc");
    }

    [Fact]
    public void UpdateAddress_WithNonExistingAddress_ShouldDoNothing()
    {
        // Arrange
        var user = CreateTestUser();
        var nonExistingId = Guid.NewGuid();

        // Act
        user.UpdateAddress(
            nonExistingId,
            "NewStreet",
            "NewCity",
            "State",
            "Country",
            "222222",
            "NewDesc"
        );

        // Assert
        user.Addresses.Should().BeEmpty(); // No exception thrown
    }

    [Fact]
    public void GetAddressesCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddAddress("Street1", "City1", "State", "Country", "111111", "Address1");
        user.AddAddress("Street2", "City2", "State", "Country", "222222", "Address2");

        // Act
        var count = user.GetAddressesCount();

        // Assert
        count.Should().Be(2);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void CanAddMoreAddresses_ShouldReturnCorrectResult(
        int existingAddresses,
        bool expectedResult
    )
    {
        // Arrange
        var user = CreateTestUser();

        for (int i = 0; i < existingAddresses; i++)
        {
            user.AddAddress($"Street{i}", $"City{i}", "State", "Country", "12345", $"Address{i}");
        }

        // Act
        var result = user.CanAddMoreAddresses();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void Addresses_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddAddress("Street1", "City1", "State", "Country", "111111", "Address1");

        // Act & Assert
        user.Addresses.Should().BeAssignableTo<IReadOnlyCollection<Address>>();
    }
}
