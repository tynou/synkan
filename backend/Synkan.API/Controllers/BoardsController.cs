using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synkan.Application.Dto.Request;
using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;

namespace Synkan.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BoardsController(
    IBoardService boardService,
    ICurrentUserService currentUser
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardDto>> Get(Guid id)
    {
        var result = await boardService.GetById(id);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<CreationResponse>> Create([FromBody] CreateBoardRequest request)
    {
        var result = await boardService.Create(request.IsPublic, request.Title);
        return Ok(new CreationResponse(result));
    }
    
    [HttpPost("{id:guid}/labels")]
    public async Task<ActionResult<CreationResponse>> CreateLabel(Guid id, [FromBody] CreateLabelRequest request)
    {
        var result = await boardService.CreateLabel(id, request.Name, request.Color);
        return Ok(new CreationResponse(result));
    }

    [HttpPost("{id:guid}/members")]
    public async Task<ActionResult> AddMember(Guid id, [FromBody] AddBoardMemberRequest request)
    {
        await boardService.AddMember(id, request.MemberId);
        return Ok();
    }
    
    [HttpDelete("{boardId:guid}/members/{memberId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid boardId, Guid memberId)
    {
        await boardService.RemoveMember(boardId, memberId);
        return Ok();
    }

    [HttpPut("{boardId:guid}/members/{memberId:guid}")]
    public async Task<ActionResult> UpdateMemberAccessLevel(Guid boardId, Guid memberId, [FromBody]  UpdateMemberAccessLevelRequest request)
    {
        await boardService.UpdateMemberAccessLevel(boardId, memberId, request.newAccessLevel);
        return Ok();
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult> UpdateTitle(Guid id, [FromBody] UpdateBoardTitleRequest request)
    {
        await boardService.UpdateTitle(id, request.Title);
        return Ok();
    }
    
    [HttpPost("{id:guid}/visibility")]
    public async Task<ActionResult> ChangeVisibility(Guid id, [FromBody] ChangeBoardVisibilityRequest request)
    {
        await boardService.ChangeVisibility(id, request.NewIsPublic);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await boardService.Delete(id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<BoardLookupDto>>> GetMyBoards()
    {
        var result = await boardService.GetBoardsByUserId(currentUser.UserId);
        return Ok(result);
    }
}