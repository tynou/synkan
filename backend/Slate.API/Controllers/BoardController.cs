using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Request;
using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BoardController(IBoardService boardService) : ControllerBase
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token Claims");
        }
        
        var result = await boardService.Create(userId, request.Title);
        
        return CreatedAtAction(nameof(Get), new { id = result }, result);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Edit(Guid id)
    {
        return NoContent();
    }
}