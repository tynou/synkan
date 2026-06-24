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
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BoardDto>> Get(Guid id)
    {
        var result = await boardService.GetById(id);
        if (result is null)
            return NotFound("Board not found");
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateBoardRequest request)
    {
        var result = await boardService.Create(currentUser.UserId, request.Title);
        
        return CreatedAtAction(nameof(Get), new { id = result }, result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id)
    {
        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await boardService.Delete(currentUser.UserId, id);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<BoardDto>>> GetMyBoards()
    {
        var result = await boardService.GetBoardsByUserId(currentUser.UserId);
        return Ok(result);
    }
}