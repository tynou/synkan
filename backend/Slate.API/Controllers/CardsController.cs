using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CardsController(ICardService cardService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardDto>> Get(Guid id)
    {
        var result = await cardService.GetById(id);
        if (result is null)
            return NotFound("Card not found");
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCardRequest request)
    {
        var result = await cardService.Create(currentUser.UserId, request.ColumnId, "title", "description");
        return CreatedAtAction("Get", "Boards", null, result); 
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await cardService.Delete(currentUser.UserId, id);
        return NoContent();
    }
}