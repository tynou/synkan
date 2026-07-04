namespace Slate.Application.Events;

public record SendMessageCommand(Guid BoardId, string Message);