namespace Synkan.Application.Dto.Response;

public record CardDto(
    Guid Id,
    Guid ColumnId,
    string Title,
    string Description,
    int Position,
    string? CoverColor,
    DateTimeOffset? DueDate,
    DateTimeOffset? ReminderDate,
    IEnumerable<ChecklistDto> Checklists,
    IEnumerable<LabelDto> Labels
);