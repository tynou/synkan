namespace Slate.Domain.Entities;

public class Column
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    
    public Board Board { get; private set; }

    private readonly List<Card> _cards = [];
    public IReadOnlyCollection<Card> Cards => _cards;

    private Column() { }
    
    public Column(Guid id, Guid boardId)
    {
        Id = id;
        BoardId = boardId;
    }
}