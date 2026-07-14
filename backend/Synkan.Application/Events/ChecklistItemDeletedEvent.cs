namespace Synkan.Application.Events;

public record ChecklistItemDeletedEvent(Guid CardId,  Guid ChecklistId, Guid ItemId);