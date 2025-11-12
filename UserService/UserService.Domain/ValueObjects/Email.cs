using UserService.Domain.Exceptions;

namespace UserService.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        InvalidEmailException.ThrowIfInvalid(value);
        Value = value.ToLower().Trim();
    }

    public string GetDomain() => Value.Split('@').Last();

    public string GetUsername() => Value.Split('@').First();

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
