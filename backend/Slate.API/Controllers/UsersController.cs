using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserListResponse>> Get([FromQuery] string username)
    {
        var result = await userService.GetAll(username);
        return Ok(new UserListResponse(result));
    }
}