namespace Slate.Domain.Entities;

public class Card
{
    public Guid Id { get; private set; }
    public Guid ColumnId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Position { get; private set; }
    
    public Column Column { get; private set; }
    
    private Card() { }
    
    public Card(Guid columnId, string title, int position)
    {
        // Id = Guid.NewGuid();
        ColumnId = columnId;
        Title = title;
        Description = string.Empty;
    }

    public void MoveToColumn(Guid newColumnId)
    {
        ColumnId = newColumnId;
    }
    
    public void UpdatePosition(int newPosition)
    {
        Position = newPosition;
    }
    
    public void SetTitle(string newTitle)
    {
        Title = newTitle;
    }
    
    public void SetDescription(string newDescription)
    {
        Description = newDescription;
    }
}