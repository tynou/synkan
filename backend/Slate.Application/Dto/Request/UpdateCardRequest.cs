namespace Slate.Application.Dto.Request;

public record UpdateCardRequest(string Title, string Description, Guid ColumnId, int Position);