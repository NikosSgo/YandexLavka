namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Mapping;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.ValueObjects;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Проверка на существование пользователя с таким телефоном или email
            var existsByPhone = await _userRepository.ExistsByPhoneAsync(request.Phone);
            if (existsByPhone)
            {
                return Result<UserDto>.Failure("User with this phone number already exists");
            }

            var existsByEmail = await _userRepository.ExistsByEmailAsync(request.Email);
            if (existsByEmail)
            {
                return Result<UserDto>.Failure("User with this email already exists");
            }

            // Создание value objects
            var phone = new Phone(request.Phone);
            var email = new Email(request.Email);

            // Создание пользователя
            var user = new User(request.FirstName, request.LastName, phone, email);

            // Сохранение в репозиторий
            await _userRepository.AddAsync(user);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}", ex);
        }
    }
}

