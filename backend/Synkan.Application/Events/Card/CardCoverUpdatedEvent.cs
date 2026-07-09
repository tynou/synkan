namespace Synkan.Application.Events;

public record CardCoverUpdatedEvent(Guid CardId, string? Color);