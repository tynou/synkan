using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synkan.Application.Dto.Response;
using Synkan.Application.Mappers;
using Synkan.Domain.Repositories;

namespace Synkan.API.Controllers;

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