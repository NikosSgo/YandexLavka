namespace AuthService.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public string InvalidEmail { get; }

    public InvalidEmailException(string email)
        : base($"Email '{email}' is invalid")
    {
        InvalidEmail = email;
    }

    public InvalidEmailException(string email, string message)
        : base(message)
    {
        InvalidEmail = email;
    }

    public static void ThrowIfInvalid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException(email, "Email cannot be empty or whitespace");

        if (!IsValidEmailFormat(email))
            throw new InvalidEmailException(email, "Email format is invalid");
    }

    private static bool IsValidEmailFormat(string email)
    {
        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return mailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}


