namespace UserService.Application.Addresses.Commands;

using FluentValidation;

public class RemoveAddressCommandValidator : AbstractValidator<RemoveAddressCommand>
{
    public RemoveAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address ID is required");
    }
}

