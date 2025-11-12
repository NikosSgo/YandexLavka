namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Domain.Interfaces.Repositories;

public class RemoveAddressCommandHandler : IRequestHandler<RemoveAddressCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public RemoveAddressCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(RemoveAddressCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetUserWithAddressesAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            var address = user.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (address == null)
            {
                return Result.Failure("Address not found");
            }

            user.RemoveAddress(request.AddressId);
            await _userRepository.UpdateAsync(user);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove address: {ex.Message}", ex);
        }
    }
}

