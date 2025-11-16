namespace AuthService.Domain.Exceptions;

public class InvalidPasswordException : DomainException
{
    public InvalidPasswordException(string message)
        : base(message)
    {
    }

    public static void ThrowIfInvalid(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidPasswordException("Password cannot be empty or whitespace");

        if (password.Length < 8)
            throw new InvalidPasswordException("Password must be at least 8 characters long");

        if (password.Length > 100)
            throw new InvalidPasswordException("Password cannot exceed 100 characters");
    }
}


