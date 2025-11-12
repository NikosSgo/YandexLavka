namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
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

