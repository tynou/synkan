using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Repositories;

namespace Slate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IIdentityService identityService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterRequest request)
    {
        var result = await identityService.Register(request.Username, request.Password);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
    {
        var result = await identityService.Login(request.Username, request.Password);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var result = await identityService.GetMe(currentUser.UserId);
        return Ok(result);
    }
}