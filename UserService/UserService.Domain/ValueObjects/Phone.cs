using UserService.Domain.Exceptions;

namespace UserService.Domain.ValueObjects;

public record Phone
{
    public string Value { get; }

    public Phone(string value)
    {
        InvalidPhoneException.ThrowIfInvalid(value);
        Value = NormalizePhone(value);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");

        if (digits.StartsWith("8") && digits.Length == 11)
        {
            return "+7" + digits.Substring(1);
        }

        if (digits.Length == 10)
        {
            return "+7" + digits;
        }

        if (phone.StartsWith("+"))
        {
            return phone;
        }

        return "+" + digits;
    }

    public override string ToString() => Value;

    public static implicit operator string(Phone phone) => phone.Value;
}
