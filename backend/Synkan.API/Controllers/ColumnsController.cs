using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synkan.Application.Dto.Request;
using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;

namespace Synkan.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ColumnsController(IColumnService columnService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ColumnDto>> Get(Guid id)
    {
        var result = await columnService.GetById(id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreationResponse>> Create([FromBody] CreateColumnRequest request)
    {
        var result = await columnService.Create(request.BoardId, request.Title);
        return Ok(new CreationResponse(result));
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> UpdateTitle(Guid id, [FromBody] UpdateColumnTitleRequest request)
    {
        await columnService.UpdateTitle(id, request.Title);
        return Ok();
    }
    
    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult> Move(Guid id, [FromBody] MoveColumnRequest request)
    {
        await columnService.Move(id, request.NewPosition);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await columnService.Delete(id);
        return NoContent();
    }
}