namespace Slate.Application.Dto.Request;

public record MoveCardRequest(Guid NewColumnId, int NewPosition);