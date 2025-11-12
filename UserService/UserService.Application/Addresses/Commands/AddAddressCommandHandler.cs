namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, Result<AddressDto>>
{
    private readonly IUserRepository _userRepository;

    public AddAddressCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AddressDto>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<AddressDto>.Failure("User not found");
            }

            if (!user.CanAddMoreAddresses())
            {
                return Result<AddressDto>.Failure("Maximum number of addresses reached (5)");
            }

            var address = user.AddAddress(
                request.Street,
                request.City,
                request.State,
                request.Country,
                request.ZipCode,
                request.Description,
                request.IsPrimary
            );

            await _userRepository.UpdateAsync(user);

            return Result<AddressDto>.Success(address.ToDto(request.UserId));
        }
        catch (Exception ex)
        {
            return Result<AddressDto>.Failure($"Failed to add address: {ex.Message}", ex);
        }
    }
}

