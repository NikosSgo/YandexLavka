namespace UserService.Domain.Entities;

using UserService.Domain.ValueObjects;

public class User : Entity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Phone Phone { get; private set; }
    public Email Email { get; private set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
    private const int MaxAddressCount = 5;

    public User(string firstName, string lastName, Phone phone, Email email)
        : base()
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Email = email;
    }

    public void ChangePhone(Phone newPhone)
    {
        Phone = newPhone;
        UpdateTimestamps();
    }

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail;
        UpdateTimestamps();
    }

    public void UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        UpdateTimestamps();
    }

    public Address AddAddress(
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string description,
        bool isPrimary = false
    )
    {
        if (_addresses.Count >= MaxAddressCount)
        {
            throw new InvalidOperationException(
                $"Cannot add more than {MaxAddressCount} addresses"
            );
        }

        if (isPrimary || _addresses.Count == 0)
        {
            foreach (var addr in _addresses.Where(a => a.IsPrimary))
            {
                addr.SetPrimary(false);
            }
        }

        var address = new Address(street, city, state, country, zipCode, description, isPrimary);
        _addresses.Add(address);
        UpdateTimestamps();
        return address;
    }

    public void RemoveAddress(Guid addressId)
    {
        var addressToRemove = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (addressToRemove != null)
        {
            _addresses.Remove(addressToRemove);
            UpdateTimestamps();
        }
    }

    public void SetPrimaryAddress(Guid addressId)
    {
        var targetAddress = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (targetAddress != null)
        {
            foreach (var addr in _addresses.Where(a => a.IsPrimary))
            {
                addr.SetPrimary(false);
            }

            targetAddress.SetPrimary(true);
            UpdateTimestamps();
        }
    }

    public Address? GetPrimaryAddress()
    {
        return _addresses.FirstOrDefault(a => a.IsPrimary);
    }

    public void UpdateAddress(
        Guid addressId,
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string description
    )
    {
        var addressToUpdate = _addresses.FirstOrDefault(a => a.Id == addressId);
        if (addressToUpdate != null)
        {
            addressToUpdate.UpdateAddress(street, city, state, country, zipCode, description);
            UpdateTimestamps();
        }
    }

    public int GetAddressesCount()
    {
        return _addresses.Count;
    }

    public bool CanAddMoreAddresses()
    {
        return _addresses.Count < MaxAddressCount;
    }
}
