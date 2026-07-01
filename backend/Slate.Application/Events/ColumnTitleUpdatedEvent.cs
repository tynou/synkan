namespace Slate.Application.Events;

public record ColumnTitleUpdatedEvent(Guid ColumnId, string Title);