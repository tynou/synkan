namespace Synkan.Application.Events;

public record ColumnTitleUpdatedEvent(Guid ColumnId, string Title);