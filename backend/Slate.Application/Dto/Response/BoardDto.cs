namespace Slate.Application.Dto.Response;

public record BoardDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    IEnumerable<UserDto> Members,
    IEnumerable<ColumnDto> Columns
);