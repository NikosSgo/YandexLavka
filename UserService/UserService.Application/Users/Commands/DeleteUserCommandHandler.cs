namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Domain.Interfaces.Repositories;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            await _userRepository.DeleteAsync(user);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete user: {ex.Message}", ex);
        }
    }
}

