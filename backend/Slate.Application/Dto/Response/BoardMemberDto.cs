using Slate.Domain.Enums;

namespace Slate.Application.Dto.Response;

public record BoardMemberDto(Guid UserId, string Username, AccessLevel AccessLevel);