using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities;

public class Account : Entity
{
    public Email Email { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private Account()
    {
        // Для EF Core
    }

    public Account(Email email, PasswordHash passwordHash)
        : base()
    {
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdateTimestamps();
    }

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail;
        UpdateTimestamps();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamps();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamps();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamps();
    }
}


