namespace Slate.Application.Dto.Response;

public record BoardLookupDto(
    Guid Id,
    Guid OwnerId,
    bool IsPublic,
    string Title,
    int MemberCount,
    int ColumnCount
);