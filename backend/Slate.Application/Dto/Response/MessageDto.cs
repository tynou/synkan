using Slate.Domain.Enums;

namespace Slate.Application.Dto.Response;

public record MessageDto(Guid MessageId, ChatMessageRole Role, string Content);