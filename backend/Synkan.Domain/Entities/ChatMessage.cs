using Synkan.Domain.Enums;

namespace Synkan.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private ChatMessage() { }

    public ChatMessage(Guid id, Guid boardId, ChatMessageRole role, string content)
    {
        Id = id;
        BoardId = boardId;
        Role = role;
        Content = content;
    }
}