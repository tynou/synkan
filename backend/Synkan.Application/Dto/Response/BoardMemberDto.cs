using Synkan.Domain.Enums;

namespace Synkan.Application.Dto.Response;

public record BoardMemberDto(Guid UserId, string Username, AccessLevel AccessLevel);