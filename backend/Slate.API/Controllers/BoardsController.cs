using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BoardsController(IBoardService boardService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("{boardId:guid}")]
    public async Task<ActionResult<BoardDto>> Get(Guid boardId)
    {
        var result = await boardService.GetById(boardId);
        if (result is null)
            return NotFound("Board not found");
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateBoardRequest request)
    {
        var result = await boardService.Create(currentUser.UserId, request.Title);
        return Ok(result);
    }

    [HttpPost("{boardId:guid}/members")]
    public async Task<ActionResult> AddMember(Guid boardId, [FromBody] AddBoardMemberRequest request)
    {
        await boardService.AddMember(currentUser.UserId, boardId, request.MemberId);
        return Ok();
    }
    
    [HttpDelete("{boardId:guid}/members/{memberId:guid}")]
    public async Task<ActionResult> RemoveMember(Guid boardId, Guid memberId)
    {
        await boardService.RemoveMember(currentUser.UserId, boardId, memberId);
        return Ok();
    }
    
    [HttpPut("{boardId:guid}")]
    public async Task<ActionResult> Update(Guid boardId, [FromBody] UpdateBoardRequest request)
    {
        await boardService.Update(currentUser.UserId, boardId, request.NewTitle);
        return Ok();
    }
    
    [HttpDelete("{boardId:guid}")]
    public async Task<ActionResult> Delete(Guid boardId)
    {
        await boardService.Delete(currentUser.UserId, boardId);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<BoardLookupDto>>> GetMyBoards()
    {
        var result = await boardService.GetBoardsByUserId(currentUser.UserId);
        return Ok(result);
    }
}