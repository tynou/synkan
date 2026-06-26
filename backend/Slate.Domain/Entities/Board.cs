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

    public void SetTitle(string newTitle)
    {
        Title = newTitle;
    }
    
    public bool UserHasAccess(Guid userId)
    {
        return OwnerId == userId || _members.Any(m => m.Id == userId);
    }
}