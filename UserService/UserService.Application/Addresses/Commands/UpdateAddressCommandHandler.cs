namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<AddressDto>>
{
    private readonly IUserRepository _userRepository;

    public UpdateAddressCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<AddressDto>.Failure("User not found");
            }

            var address = user.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (address == null)
            {
                return Result<AddressDto>.Failure("Address not found");
            }

            user.UpdateAddress(
                request.AddressId,
                request.Street,
                request.City,
                request.State,
                request.Country,
                request.ZipCode,
                request.Description
            );

            await _userRepository.UpdateAsync(user);

            var updatedAddress = user.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (updatedAddress == null)
            {
                return Result<AddressDto>.Failure("Address not found after update");
            }

            return Result<AddressDto>.Success(updatedAddress.ToDto(request.UserId));
        }
        catch (Exception ex)
        {
            return Result<AddressDto>.Failure($"Failed to update address: {ex.Message}", ex);
        }
    }
}

