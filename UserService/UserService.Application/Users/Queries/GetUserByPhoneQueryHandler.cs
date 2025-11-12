namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetUserByPhoneQueryHandler : IRequestHandler<GetUserByPhoneQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByPhoneQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByPhoneQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByPhoneAsync(request.Phone);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var userWithAddresses = await _userRepository.GetUserWithAddressesAsync(user.Id);
            if (userWithAddresses == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            return Result<UserDto>.Success(userWithAddresses.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to get user: {ex.Message}", ex);
        }
    }
}

