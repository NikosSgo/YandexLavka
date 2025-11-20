namespace OrderService.Application.Dtos;

public record DeliveryAddressDto(
    string Country,
    string City,
    string Street,
    string Building,
    string? Apartment,
    string? Comment);

