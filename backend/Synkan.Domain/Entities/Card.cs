namespace Synkan.Domain.Entities;

public class Card
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid ColumnId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int Position { get; private set; }
    
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? ReminderTime { get; private set; }
    public string? ReminderJobId { get; private set; }
    
    public string? CoverColor { get; private set; }

    private readonly List<Checklist> _checklists = [];
    public IReadOnlyCollection<Checklist> Checklists => _checklists.AsReadOnly();

    private readonly List<Label> _labels = [];
    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();
    
    public Column Column { get; private set; }
    
    private Card() { }
    
    public Card(Guid columnId, Guid boardId, string title, int position)
    {
        // Id = Guid.NewGuid();
        ColumnId = columnId;
        BoardId = boardId;
        Title = title;
        Description = string.Empty;
        Position = position;
    }
    
    public void UpdateDeadline(DateTimeOffset dueDate, DateTimeOffset reminderTime, string newJobId)
    {
        DueDate = dueDate;
        ReminderTime = reminderTime;
        ReminderJobId = newJobId;
    }

    public void RemoveDeadline()
    {
        DueDate = null;
        ReminderTime = null;
        ReminderJobId = null;
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
    
    public void UpdateCoverColor(string? color)
    {
        CoverColor = color;
    }

    public Checklist AddChecklist(string title)
    {
        var checklist = new Checklist(Id, title);
        _checklists.Add(checklist);
        return checklist;
    }

    public void RemoveChecklist(Guid checklistId)
    {
        var checklist = _checklists.FirstOrDefault(cl => cl.Id == checklistId);
        if (checklist is null)
            return;
        _checklists.Remove(checklist);
    }

    public ChecklistItem AddChecklistItem(Guid checklistId, string text)
    {
        var checklist = _checklists.FirstOrDefault(cl => cl.Id == checklistId);
        return checklist?.AddItem(text);
    }

    public void RemoveChecklistItem(Guid checklistId, Guid itemId)
    {
        var checklist = _checklists.FirstOrDefault(cl => cl.Id == checklistId);
        checklist?.RemoveItem(itemId);
    }

    public void ToggleChecklistItem(Guid checklistId, Guid itemId)
    {
        var checklist = _checklists.FirstOrDefault(cl => cl.Id == checklistId);
        checklist?.ToggleItem(itemId);
    }

    public void AssignLabel(Label label)
    {
        if (label.BoardId != BoardId)
            throw new InvalidOperationException("Label belongs to a different board.");

        if (_labels.All(l => l.Id != label.Id))
            _labels.Add(label);
    }

    public void RemoveLabel(Guid labelId)
    {
        var label = _labels.FirstOrDefault(l => l.Id == labelId);
        if (label is not null)
            _labels.Remove(label);
    }
}