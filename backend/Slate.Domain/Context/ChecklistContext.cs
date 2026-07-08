namespace Slate.Domain.Context;

public record ChecklistContext(
    Guid Id,
    string Title,
    IEnumerable<ChecklistItemContext> Items
);