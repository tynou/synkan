namespace Slate.Application.Events;

public record MessageChunkEvent(Guid BoardId, Guid MessageId, string Chunk);