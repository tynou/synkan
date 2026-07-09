namespace Synkan.Application.Events;

public record CardMovedEvent(Guid CardId, Guid ColumnId, int Position);