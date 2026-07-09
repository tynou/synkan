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
    
    [HttpPut("{id:guid}/due")]
    public async Task<ActionResult> UpdateDueDate(Guid id, [FromBody] UpdateCardDueDateRequest request)
    {
        await cardService.UpdateDueDate(id, request.DueDate, request.ReminderTime);
        return Ok();
    }
    
    [HttpDelete("{id:guid}/due")]
    public async Task<ActionResult> RemoveDueDate(Guid id)
    {
        await cardService.RemoveDueDate(id);
        return Ok();
    }
    
    [HttpPost("{id:guid}/labels/{labelId:guid}")]
    public async Task<ActionResult> AssignLabel(Guid id, Guid labelId)
    {
        await cardService.AssignLabel(id, labelId);
        return Ok();
    }
    
    [HttpDelete("{id:guid}/labels/{labelId:guid}")]
    public async Task<ActionResult> RemoveLabel(Guid id, Guid labelId)
    {
        await cardService.RemoveLabel(id, labelId);
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