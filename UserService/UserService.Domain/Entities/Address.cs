namespace UserService.Domain.Entities;

public class Address : Entity
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }
    public string Description { get; private set; }
    public bool IsPrimary { get; private set; }

    public Address(
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string description,
        bool isPrimary
    )
        : base()
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
        Description = description;
        IsPrimary = isPrimary;
    }

    public override string ToString()
    {
        return $"{Street}, {City}, {State}, {Country}, {ZipCode}";
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdateTimestamps();
    }

    public void UpdateAddress(
        string street,
        string city,
        string state,
        string country,
        string zipCode,
        string description
    )
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
        Description = description;
        UpdateTimestamps();
    }
}
