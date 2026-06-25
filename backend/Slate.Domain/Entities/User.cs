namespace Slate.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash {get; private set;}

    private User() { }
    
    public User(Guid id, string username, string passwordHash)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
    }
}