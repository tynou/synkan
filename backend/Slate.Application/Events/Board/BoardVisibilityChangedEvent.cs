namespace Slate.Application.Events;

public record BoardVisibilityChangedEvent(Guid BoardId, bool IsPublic);