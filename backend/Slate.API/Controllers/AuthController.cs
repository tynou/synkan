using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IIdentityService identityService, ICurrentUserService currentUser, IAuthCookieService authCookieService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var token = await identityService.Register(request.Username, request.Password);
        authCookieService.SetAuthCookie(Response, token);
        return Ok(new AuthResponse(token));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var token = await identityService.Login(request.Username, request.Password);
        authCookieService.SetAuthCookie(Response, token);
        return Ok(new AuthResponse(token));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var result = await identityService.GetMe(currentUser.UserId);
        return Ok(result);
    }
}