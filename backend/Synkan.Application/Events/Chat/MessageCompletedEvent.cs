namespace Synkan.Application.Events;

public record MessageCompletedEvent(Guid BoardId, Guid MessageId);