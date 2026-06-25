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
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ColumnDto>> Get(Guid id)
    {
        var result = await columnService.GetById(id);
        if (result is null)
            return NotFound("Column not found");
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateColumnRequest request)
    {
        var result = await columnService.Create(currentUser.UserId, request.BoardId, request.Title);
        return CreatedAtAction("Get", "Boards", new { id = request.BoardId }, result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id)
    {
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await columnService.Delete(currentUser.UserId, id);
        return NoContent();
    }
}