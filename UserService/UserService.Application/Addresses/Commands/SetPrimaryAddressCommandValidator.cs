namespace UserService.Application.Addresses.Commands;

using FluentValidation;

public class SetPrimaryAddressCommandValidator : AbstractValidator<SetPrimaryAddressCommand>
{
    public SetPrimaryAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address ID is required");
    }
}

