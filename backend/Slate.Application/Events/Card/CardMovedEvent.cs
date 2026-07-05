namespace Slate.Application.Events;

public record CardMovedEvent(Guid CardId, Guid ColumnId, int Position);