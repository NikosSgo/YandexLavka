namespace UserService.Application.Addresses.Commands;

using FluentValidation;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address ID is required");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(100)
            .WithMessage("State must not exceed 100 characters");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("Zip code is required")
            .MaximumLength(20)
            .WithMessage("Zip code must not exceed 20 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

