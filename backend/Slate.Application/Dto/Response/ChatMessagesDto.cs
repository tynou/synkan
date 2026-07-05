namespace Slate.Application.Dto.Response;

public record ChatMessagesDto(IEnumerable<MessageDto> Messages);