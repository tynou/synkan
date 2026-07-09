namespace Synkan.Application.Events;

public record CardContentUpdatedEvent(Guid CardId, string Title, string Description);