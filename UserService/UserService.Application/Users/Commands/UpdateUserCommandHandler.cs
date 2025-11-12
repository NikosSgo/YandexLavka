namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.ValueObjects;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Обновление имени
            if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
            {
                var firstName = request.FirstName ?? user.FirstName;
                var lastName = request.LastName ?? user.LastName;
                user.UpdateName(firstName, lastName);
            }

            // Обновление телефона
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var existsByPhone = await _userRepository.ExistsByPhoneAsync(request.Phone);
                if (existsByPhone && user.Phone.Value != request.Phone)
                {
                    return Result<UserDto>.Failure("User with this phone number already exists");
                }

                var phone = new Phone(request.Phone);
                user.ChangePhone(phone);
            }

            // Обновление email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var existsByEmail = await _userRepository.ExistsByEmailAsync(request.Email);
                if (existsByEmail && user.Email.Value != request.Email)
                {
                    return Result<UserDto>.Failure("User with this email already exists");
                }

                var email = new Email(request.Email);
                user.ChangeEmail(email);
            }

            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to update user: {ex.Message}", ex);
        }
    }
}

