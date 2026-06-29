using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ColumnsController(IColumnService columnService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{columnId:guid}")]
    public async Task<ActionResult<ColumnDto>> Get(Guid columnId)
    {
        var result = await columnService.GetById(currentUser.UserId, columnId);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreationResponse>> Create([FromBody] CreateColumnRequest request)
    {
        var result = await columnService.Create(currentUser.UserId, request.BoardId, request.Title);
        return Ok(new CreationResponse(result));
    }
    
    [HttpPut("{columnId:guid}")]
    public async Task<ActionResult> Update(Guid columnId, [FromBody] UpdateColumnRequest request)
    {
        await columnService.Update(currentUser.UserId, columnId, request.Title, request.Position);
        return Ok();
    }

    [HttpDelete("{columnId:guid}")]
    public async Task<ActionResult> Delete(Guid columnId)
    {
        await columnService.Delete(currentUser.UserId, columnId);
        return NoContent();
    }
}