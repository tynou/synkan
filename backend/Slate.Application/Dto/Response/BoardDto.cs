namespace Slate.Application.Dto.Response;

public record BoardDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    IEnumerable<BoardMemberDto> Members,
    IEnumerable<ColumnDto> Columns
);