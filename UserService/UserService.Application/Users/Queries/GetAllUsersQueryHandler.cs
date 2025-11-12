namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userWithAddresses = await _userRepository.GetUserWithAddressesAsync(user.Id);
                if (userWithAddresses != null)
                {
                    userDtos.Add(userWithAddresses.ToDto());
                }
            }

            return Result<List<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            return Result<List<UserDto>>.Failure($"Failed to get users: {ex.Message}", ex);
        }
    }
}

