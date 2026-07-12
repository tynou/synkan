using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synkan.Application.Dto.Request;
using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;

namespace Synkan.API.Controllers;

[ApiController]
[Authorize]
[Route("api/boards/{boardId:guid}/[controller]")]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BoardAiSettingsDto>> GetBoardAiSettings([FromRoute] Guid boardId)
    {
        var result = await settingsService.GetOrCreateAsync(boardId);
        return Ok(result);
    }
    
    [HttpPut]
    public async Task<ActionResult> UpdateBoardAiSettings([FromRoute] Guid boardId, [FromBody] UpdateBoardAiSettingsRequest request)
    {
        await settingsService.UpdateAsync(boardId, request.ApiKey, request.Provider, request.Model);
        return Ok();
    }
}