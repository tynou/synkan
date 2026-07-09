namespace Synkan.Application.Dto.Response;

public record ChatMessagesDto(IEnumerable<MessageDto> Messages);