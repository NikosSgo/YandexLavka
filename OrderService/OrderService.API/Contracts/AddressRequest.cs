namespace OrderService.API.Contracts;

public record AddressRequest(
    string Country,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string? Comment);

