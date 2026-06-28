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
    [HttpGet("{cardId:guid}")]
    public async Task<ActionResult<CardDto>> Get(Guid cardId)
    {
        var result = await cardService.GetById(cardId);
        if (result is null)
            return NotFound("Card not found");
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCardRequest request)
    {
        var result = await cardService.Create(currentUser.UserId, request.ColumnId, request.Title);
        return Ok(result);
    }
    
    [HttpPut("{cardId:guid}")]
    public async Task<ActionResult> Update(Guid cardId, [FromBody] UpdateCardRequest request)
    {
        await cardService.Update(
            currentUser.UserId,
            cardId,
            request.Title,
            request.Description,
            request.ColumnId,
            request.Position
        );
        return Ok();
    }
    
    [HttpDelete("{cardId:guid}")]
    public async Task<ActionResult> Delete(Guid cardId)
    {
        await cardService.Delete(currentUser.UserId, cardId);
        return NoContent();
    }
}