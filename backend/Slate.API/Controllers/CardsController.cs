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
        var result = await cardService.GetById(currentUser.UserId, id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreationResponse>> Create([FromBody] CreateCardRequest request)
    {
        var result = await cardService.Create(currentUser.UserId, request.ColumnId, request.Title);
        return Ok(new CreationResponse(result));
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> UpdateContent(Guid id, [FromBody] UpdateCardContentRequest request)
    {
        await cardService.UpdateContent(currentUser.UserId, id, request.Title, request.Description);
        return Ok();
    }
    
    [HttpPut("{id:guid}/cover")]
    public async Task<ActionResult> UpdateContent(Guid id, [FromBody] UpdateCardCoverRequest request)
    {
        await cardService.UpdateCover(currentUser.UserId, id, request.Color);
        return Ok();
    }
    
    [HttpPost("{id:guid}/move")]
    public async Task<ActionResult> Move(Guid id, [FromBody] MoveCardRequest request)
    {
        await cardService.Move(currentUser.UserId, id, request.NewColumnId, request.NewPosition);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await cardService.Delete(currentUser.UserId, id);
        return NoContent();
    }
}