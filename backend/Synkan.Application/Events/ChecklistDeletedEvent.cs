namespace Synkan.Application.Events;

public record ChecklistDeletedEvent(Guid CardId, Guid ChecklistId);