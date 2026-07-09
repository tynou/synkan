namespace Synkan.Domain.Context;

public record ChecklistItemContext(
    Guid Id,
    string Text,
    bool IsCompleted
);