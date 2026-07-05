namespace Slate.Application.Events;

public record BoardTitleUpdatedEvent(Guid BoardId, string Title);