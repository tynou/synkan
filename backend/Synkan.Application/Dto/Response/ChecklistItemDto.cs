namespace Synkan.Application.Dto.Response;

public record ChecklistItemDto(
    Guid Id,
    Guid ChecklistId,
    string Text,
    bool IsCompleted,
    int Position
);