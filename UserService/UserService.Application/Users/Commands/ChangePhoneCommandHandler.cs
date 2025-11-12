namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.ValueObjects;

public class ChangePhoneCommandHandler : IRequestHandler<ChangePhoneCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public ChangePhoneCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(ChangePhoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            var existsByPhone = await _userRepository.ExistsByPhoneAsync(request.Phone);
            if (existsByPhone && user.Phone.Value != request.Phone)
            {
                return Result<UserDto>.Failure("User with this phone number already exists");
            }

            var phone = new Phone(request.Phone);
            user.ChangePhone(phone);

            await _userRepository.UpdateAsync(user);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to change phone: {ex.Message}", ex);
        }
    }
}

