using Synkan.Domain.Enums;

namespace Synkan.Application.Dto.Response;

public record MessageDto(Guid MessageId, ChatMessageRole Role, string Content);