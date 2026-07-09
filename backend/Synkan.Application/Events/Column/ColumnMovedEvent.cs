namespace Synkan.Application.Events;

public record ColumnMovedEvent(Guid ColumnId, int Position);