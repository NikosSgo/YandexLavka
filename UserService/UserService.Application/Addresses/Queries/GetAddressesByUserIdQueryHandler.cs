namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetAddressesByUserIdQueryHandler : IRequestHandler<GetAddressesByUserIdQuery, Result<List<AddressDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetAddressesByUserIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<AddressDto>>> Handle(GetAddressesByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<List<AddressDto>>.Failure("User not found");
            }

            var addressDtos = user.Addresses.Select(a => a.ToDto(request.UserId)).ToList();

            return Result<List<AddressDto>>.Success(addressDtos);
        }
        catch (Exception ex)
        {
            return Result<List<AddressDto>>.Failure($"Failed to get addresses: {ex.Message}", ex);
        }
    }
}

