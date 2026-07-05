namespace Slate.Application.Events;

public record MessageCompletedEvent(Guid BoardId, Guid MessageId);