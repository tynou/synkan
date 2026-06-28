using Slate.Domain.Enums;

namespace Slate.Domain.Entities;

public class BoardMember
{
    public Guid BoardId { get; private set; }
    public Guid UserId { get; private set; }
    public AccessLevel AccessLevel { get; private set; }
    
    public User User { get; private set; }
    
    private BoardMember() { }

    public BoardMember(Guid boardId, Guid userId, AccessLevel accessLevel)
    {
        BoardId = boardId;
        UserId = userId;
        AccessLevel = accessLevel;
    }
    
    public void UpdateAccessLevel(AccessLevel newLevel)
    {
        AccessLevel = newLevel;
    }
}