namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.ValueObjects;

public class ChangeEmailCommandHandler : IRequestHandler<ChangeEmailCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public ChangeEmailCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var existsByEmail = await _userRepository.ExistsByEmailAsync(request.Email);
            if (existsByEmail && user.Email.Value != request.Email)
            {
                return Result<UserDto>.Failure("User with this email already exists");
            }

            var email = new Email(request.Email);
            user.ChangeEmail(email);

            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to change email: {ex.Message}", ex);
        }
    }
}

