namespace Slate.Application.Dto.Response;

public record ColumnDto(
    Guid Id,
    Guid BoardId,
    string Title,
    int Position,
    IEnumerable<CardDto> Cards
);