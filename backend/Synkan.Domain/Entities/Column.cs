namespace Synkan.Domain.Entities;

public class Column
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public string Title { get; private set; }
    public int Position { get; private set; }
    
    public Board Board { get; private set; }

    private readonly List<Card> _cards = [];
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

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

        var card = new Card(Id, BoardId, title, nextPosition);
        _cards.Add(card);

        return card;
    }
    
    public void RemoveCard(Card card)
    {
        _cards.Remove(card);
        
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].UpdatePosition(i); 
        }
    }

    public void MoveCard(Card card, int newPosition)
    {
        if (card.Position == newPosition) return;
        
        newPosition = Math.Max(0, Math.Min(newPosition, _cards.Count - 1));
        
        _cards.Remove(card);
        _cards.Insert(newPosition, card);
        
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].UpdatePosition(i);
        }
    }
    
    public void InsertCard(Card card, int newPosition)
    {
        newPosition = Math.Clamp(newPosition, 0, _cards.Count);

        _cards.Insert(newPosition, card);
        
        card.MoveToColumn(Id);

        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].UpdatePosition(i);
        }
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