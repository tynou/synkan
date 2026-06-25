namespace Slate.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; }

    private readonly List<User> _members = [];
    public IReadOnlyCollection<User> Members => _members.AsReadOnly();

    private readonly List<Column> _columns = [];
    public IReadOnlyCollection<Column> Columns => _columns.AsReadOnly();
    
    private Board() { }
    
    public Board(User owner, string title)
    {
        // Id = Guid.NewGuid();
        OwnerId = owner.Id;
        Title = title;
        
        AddMember(owner);
    }
    
    public void AddMember(User user)
    { 
        if (_members.All(m => m.Id != user.Id))
            _members.Add(user);
    }
    
    public Column AddColumn(string title)
    {
        var nextPosition = _columns.Count;

        var column = new Column(Id, title, nextPosition);
        _columns.Add(column);

        return column;
    }
    
    public bool UserHasAccess(Guid userId)
    {
        return OwnerId == userId || _members.Any(m => m.Id == userId);
    }
}