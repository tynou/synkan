namespace Synkan.Application.Events;

public record ChecklistItemToggledEvent(Guid CardId, Guid ChecklistId, Guid ItemId);