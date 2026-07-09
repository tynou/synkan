namespace Synkan.Application.Events;

public record BoardTitleUpdatedEvent(Guid BoardId, string Title);