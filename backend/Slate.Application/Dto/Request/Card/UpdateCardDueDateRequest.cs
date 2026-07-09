namespace Slate.Application.Dto.Request;

public record UpdateCardDueDateRequest(DateTimeOffset DueDate, DateTimeOffset ReminderTime);