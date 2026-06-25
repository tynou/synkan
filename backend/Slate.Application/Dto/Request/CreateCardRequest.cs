namespace Slate.Application.Dto.Request;

public record CreateCardRequest(Guid BoardId, Guid ColumnId, string Title);