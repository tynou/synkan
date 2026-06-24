namespace Slate.Domain.Entities;

public class Card
{
    public Guid Id { get; private set; }
    public Guid ColumnId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    
    public Column Column { get; private set; }
    
    private Card() { }
    
    public Card(Guid id, Guid columnId, string title)
    {
        Id = id;
        ColumnId = columnId;
        Title = title;
        Description = string.Empty;
    }
}