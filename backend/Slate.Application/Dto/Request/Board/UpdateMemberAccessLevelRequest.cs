using Slate.Domain.Enums;

namespace Slate.Application.Dto.Request;

public record UpdateMemberAccessLevelRequest(AccessLevel newAccessLevel);