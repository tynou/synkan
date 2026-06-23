namespace Slate.Application.Dto.Response;

public record ColumnDto(
    Guid Id,
    Guid BoardId,
    IEnumerable<CardDto> Cards
);