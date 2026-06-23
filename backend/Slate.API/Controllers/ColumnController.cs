using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ColumnController(IColumnService columnService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateColumnRequest request)
    {
        var result = await columnService.Create(currentUser.UserId, request.BoardId);
        return CreatedAtAction("Get", "Board", new { id = request.BoardId }, result);
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