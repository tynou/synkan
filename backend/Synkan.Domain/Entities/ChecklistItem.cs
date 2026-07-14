namespace Synkan.Domain.Entities;

public class ChecklistItem
{
    public Guid Id { get; private set; }
    public Guid ChecklistId { get; private set; }
    public string Text { get; private set; }
    public bool IsCompleted { get; private set; }
    public int Position { get; private set; }

    private ChecklistItem() { }

    public ChecklistItem(Guid checklistId, string text, int position)
    {
        ChecklistId = checklistId;
        Text = text;
        Position = position;
        IsCompleted = false;
    }

    public void Toggle()
    {
        IsCompleted = !IsCompleted;
    }
}