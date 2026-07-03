namespace Slate.Application.Dto.Response;

public record CardDto(
    Guid Id,
    Guid ColumnId,
    string Title,
    string Description,
    int Position,
    string? CoverColor,
    IEnumerable<ChecklistDto> Checklists
);