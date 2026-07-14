namespace Synkan.Application.Events;

public record SendMessageCommand(Guid BoardId, string Message);