namespace Synkan.Domain.Entities;

public class Checklist
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public string Title { get; private set; }

    private readonly List<ChecklistItem> _items = [];
    public IReadOnlyCollection<ChecklistItem> Items => _items.AsReadOnly();

    private Checklist() { }

    public Checklist(Guid cardId, string title)
    {
        CardId = cardId;
        Title = title;
    }

    public ChecklistItem AddItem(string text)
    {
        var item = new ChecklistItem(Id, text, _items.Count);
        _items.Add(item);
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(cl => cl.Id == itemId);
        if (item is null)
            return;
        _items.Remove(item);
    }

    public void ToggleItem(Guid itemId, bool isCompleted)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        item?.Toggle(isCompleted);
    }
}