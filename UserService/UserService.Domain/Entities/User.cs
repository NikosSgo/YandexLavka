namespace UserService.Domain.Entities;

using UserService.Domain.Common;

public class User : Entity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }

    private readonly List<Address> _addresses = new();
    public IReadOnlyCollection<Address> Address { get; private set; }

    public User(string firstName, string lastName, string phone, string email)
        : base()
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Email = email;
    }

    public void ChangePhone(string newPhone)
    {
        Phone = newPhone;
    }
}
