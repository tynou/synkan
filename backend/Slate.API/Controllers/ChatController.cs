using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slate.Application.Dto.Response;
using Slate.Application.Mappers;
using Slate.Domain.Repositories;

namespace Slate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/boards/{boardId:guid}/[controller]")]
public class ChatController(IChatMessageRepository messageRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ChatMessagesDto>> GetMessages(Guid boardId)
    {
        var messages = await messageRepository.GetByBoardIdAsync(boardId);
        return Ok(new ChatMessagesDto(messages.Select(m => m.ToDto())));
    }
}