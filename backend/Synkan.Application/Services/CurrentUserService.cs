using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Synkan.Application.Interfaces;

namespace Synkan.Application.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(user?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var userId)
        ? userId
        : throw new UnauthorizedAccessException("User is not authenticated.");
}