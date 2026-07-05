namespace Slate.Application.Events;

public record CardCoverUpdatedEvent(Guid CardId, string? Color);