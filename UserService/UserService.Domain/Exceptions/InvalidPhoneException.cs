// UserService.Domain/Exceptions/InvalidPhoneException.cs
namespace UserService.Domain.Exceptions;

public class InvalidPhoneException : DomainException
{
    public string InvalidPhone { get; }

    public InvalidPhoneException(string phone)
        : base($"Phone number '{phone}' is invalid")
    {
        InvalidPhone = phone;
    }

    public InvalidPhoneException(string phone, string message)
        : base(message)
    {
        InvalidPhone = phone;
    }

    public static void ThrowIfInvalid(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new InvalidPhoneException(phone, "Phone number cannot be empty or whitespace");

        // Базовая проверка формата (можно адаптировать под ваши требования)
        if (!IsValidPhoneFormat(phone))
            throw new InvalidPhoneException(phone, "Phone number format is invalid");
    }

    private static bool IsValidPhoneFormat(string phone)
    {
        // Убираем все нецифровые символы кроме +
        var cleanedPhone = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");

        // Проверяем минимальную и максимальную длину
        if (cleanedPhone.Length < 10 || cleanedPhone.Length > 15)
            return false;

        // Проверяем, что номер состоит из цифр (может начинаться с +)
        return System.Text.RegularExpressions.Regex.IsMatch(cleanedPhone, @"^\+?[\d\s\-\(\)]+$");
    }
}
