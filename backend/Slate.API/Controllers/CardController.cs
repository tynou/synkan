using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CardController(ICardService cardService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCardRequest request)
    {
        var result = await cardService.Create(currentUser.UserId, request.ColumnId, "title", "description");
        return CreatedAtAction("Get", "Board", null, result); 
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await cardService.Delete(currentUser.UserId, id);
        return NoContent();
    }
}