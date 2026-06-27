namespace Slate.Application.Dto.Response;

public record BoardLookupDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    int MemberCount,
    int ColumnCount
);