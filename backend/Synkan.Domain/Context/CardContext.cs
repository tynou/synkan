namespace Synkan.Domain.Context;

public record CardContext(
    Guid Id,
    string Title,
    string Description,
    int Position,
    string? CoverColor,
    IEnumerable<ChecklistContext> Checklists,
    IEnumerable<LabelContext> Labels
);