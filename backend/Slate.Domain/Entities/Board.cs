namespace Slate.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Title { get; private set; }

    private readonly List<Column> _columns = [];
    public IReadOnlyCollection<Column> Columns => _columns;
    
    public  Board(Guid id, Guid ownerId, string title)
    {
        Id = id;
        OwnerId = ownerId;
        Title = title;
    }
}