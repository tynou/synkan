namespace Synkan.Application.Dto.Response;

public record BoardDto(
    Guid Id,
    Guid OwnerId,
    bool IsPublic,
    string Title,
    IEnumerable<BoardMemberDto> Members,
    IEnumerable<ColumnDto> Columns,
    IEnumerable<LabelDto> Labels
);