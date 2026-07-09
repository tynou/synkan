using Synkan.Domain.Enums;

namespace Synkan.Application.Dto.Request;

public record UpdateMemberAccessLevelRequest(AccessLevel newAccessLevel);