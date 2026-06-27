using Slate.Domain.Enums;

namespace Slate.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; }

    private readonly List<BoardMember> _members = [];
    public IReadOnlyCollection<BoardMember> Members => _members.AsReadOnly();

    private readonly List<Column> _columns = [];
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    
    private Board() { }
    
    public Board(User owner, string title)
    {
        // Id = Guid.NewGuid();
        OwnerId = owner.Id;
        Title = title;
        
        AddMember(owner.Id, AccessLevel.Admin);
    }
    
    public void AddMember(Guid userId, AccessLevel accessLevel)
    { 
        if (_members.All(m => m.UserId != userId))
            _members.Add(new BoardMember(Id, userId, accessLevel));
    }

    public void RemoveMember(Guid userId)
    {
        if (userId == OwnerId) return;
        
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is null) return;

        _members.Remove(member);
    }
    
    public Column AddColumn(string title)
    {
        var nextPosition = _columns.Count;

        var column = new Column(Id, title, nextPosition);
        _columns.Add(column);

        return column;
    }
    
    public void RemoveColumn(Guid columnId)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null) return;

        _columns.Remove(column);
        
        for (var i = 0; i < _columns.Count; i++)
        {
            _columns[i].UpdatePosition(i); 
        }
    }
    
    public void MoveColumn(Guid columnId, int newPosition)
    {
        var column = _columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null) return;

        if (column.Position == newPosition) return;
        
        newPosition = Math.Max(0, Math.Min(newPosition, _columns.Count - 1));
        
        _columns.Remove(column);
        _columns.Insert(newPosition, column);
        
        for (var i = 0; i < _columns.Count; i++)
        {
            _columns[i].UpdatePosition(i);
        }
    }

    public void SetTitle(string newTitle)
    {
        Title = newTitle;
    }
    
    public bool UserHasAccess(Guid userId)
    {
        return OwnerId == userId || _members.Any(m => m.UserId == userId);
    }
}