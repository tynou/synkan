namespace Slate.Application.Events;

public record MessageChunkEvent(Guid BoardId, string Chunk);