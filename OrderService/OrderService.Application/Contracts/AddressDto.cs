namespace OrderService.Application.Contracts;

public record AddressDto(
    string Country,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string? Comment);

