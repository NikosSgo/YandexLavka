namespace OrderService.Domain.ValueObjects;

public record Address(
    string Country,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string? Comment
);

