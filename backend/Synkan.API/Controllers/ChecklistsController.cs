using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synkan.Application.Dto.Request;
using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;

namespace Synkan.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cards/{cardId:guid}/[controller]")]
public class ChecklistsController(IChecklistService checklistService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreationResponse>> Create(Guid cardId, [FromBody] CreateChecklistRequest request)
    {
        var result = await checklistService.Create(cardId, request.Title);
        return Ok(new CreationResponse(result));
    }
    
    [HttpPost("{checklistId:guid}/items")]
    public async Task<ActionResult<CreationResponse>> CreateItem(Guid cardId, Guid checklistId, [FromBody] CreateChecklistItemRequest request)
    {
        var result = await checklistService.CreateItem(cardId, checklistId, request.Text);
        return Ok(new CreationResponse(result));
    }

    [HttpPut("{checklistId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult> ToggleItem(Guid cardId, Guid checklistId, Guid itemId, [FromBody] ToggleChecklistItemRequest request)
    {
        await checklistService.ToggleItem(cardId, checklistId, itemId, request.IsCompleted);
        return Ok();
    }
    
    [HttpDelete("{checklistId:guid}")]
    public async Task<ActionResult> Delete(Guid cardId, Guid checklistId)
    {
        await checklistService.Delete(cardId, checklistId);
        return NoContent();
    }
    
    [HttpDelete("{checklistId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult> DeleteItem(Guid cardId, Guid checklistId, Guid itemId)
    {
        await checklistService.DeleteItem(cardId, checklistId, itemId);
        return NoContent();
    }
}