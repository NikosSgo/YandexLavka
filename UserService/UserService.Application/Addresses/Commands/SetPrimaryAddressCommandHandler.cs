namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Domain.Interfaces.Repositories;

public class SetPrimaryAddressCommandHandler : IRequestHandler<SetPrimaryAddressCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public SetPrimaryAddressCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(SetPrimaryAddressCommand request, CancellationToken cancellationToken)
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

            user.SetPrimaryAddress(request.AddressId);
            await _userRepository.UpdateAsync(user);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to set primary address: {ex.Message}", ex);
        }
    }
}

