namespace Slate.Domain.Entities;

public class Column
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public string Title { get; private set; }
    public int Position { get; private set; }
    
    public Board Board { get; private set; }

    private readonly List<Card> _cards = [];
    public IReadOnlyCollection<Card> Cards => _cards;

    private Column() { }
    
    public Column(Guid boardId, string title, int position)
    {
        // Id = Guid.NewGuid();
        BoardId = boardId;
        Title = title;
        Position = position;
    }
    
    public Card AddCard(string title)
    {
        var nextPosition = _cards.Count;

        var card = new Card(Id, title, nextPosition);
        _cards.Add(card);

        return card;
    }

    public void UpdatePosition(int newPosition)
    {
        Position = newPosition;
    }
    
    public void SetTitle(string newTitle)
    {
        Title = newTitle;
    }
}