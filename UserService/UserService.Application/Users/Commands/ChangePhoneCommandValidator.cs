namespace UserService.Application.Users.Commands;

using FluentValidation;

public class ChangePhoneCommandValidator : AbstractValidator<ChangePhoneCommand>
{
    public ChangePhoneCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone is required")
            .MaximumLength(20)
            .WithMessage("Phone must not exceed 20 characters");
    }
}

