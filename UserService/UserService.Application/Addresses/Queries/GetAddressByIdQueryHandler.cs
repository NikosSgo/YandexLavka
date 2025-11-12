namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, Result<AddressDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAddressByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AddressDto>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
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

            return Result<AddressDto>.Success(address.ToDto(request.UserId));
        }
        catch (Exception ex)
        {
            return Result<AddressDto>.Failure($"Failed to get address: {ex.Message}", ex);
        }
    }
}

