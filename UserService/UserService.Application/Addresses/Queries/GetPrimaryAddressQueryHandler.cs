namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetPrimaryAddressQueryHandler : IRequestHandler<GetPrimaryAddressQuery, Result<AddressDto?>>
{
    private readonly IUserRepository _userRepository;

    public GetPrimaryAddressQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AddressDto?>> Handle(GetPrimaryAddressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<AddressDto?>.Failure("User not found");
            }

            var primaryAddress = user.GetPrimaryAddress();
            if (primaryAddress == null)
            {
                return Result<AddressDto?>.Success(null);
            }

            return Result<AddressDto?>.Success(primaryAddress.ToDto(request.UserId));
        }
        catch (Exception ex)
        {
            return Result<AddressDto?>.Failure($"Failed to get primary address: {ex.Message}", ex);
        }
    }
}

