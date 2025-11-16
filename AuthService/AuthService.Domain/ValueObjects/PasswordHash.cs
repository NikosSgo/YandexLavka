namespace AuthService.Domain.ValueObjects;

public record PasswordHash
{
    public string Value { get; }

    public PasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty", nameof(hash));

        Value = hash;
    }

    public override string ToString() => Value;

    public static implicit operator string(PasswordHash passwordHash) => passwordHash.Value;
}


