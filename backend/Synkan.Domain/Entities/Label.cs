namespace Synkan.Domain.Entities;

public class Label
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public string Name { get; private set; }
    public string Color { get; private set; }

    private Label() { }

    public Label(Guid boardId, string name, string color)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        Name = name;
        Color = color;
    }
}