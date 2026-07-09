namespace Synkan.Application.Dto.Response;

public record ChecklistDto(
    Guid Id,
    Guid CardId,
    string Title,
    IEnumerable<ChecklistItemDto> Items
);