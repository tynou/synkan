using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;
using Slate.Application.Events;
using Slate.Application.Interfaces;

namespace Slate.Application.Hubs;

[AsyncApi]
[Authorize]
public class BoardHub(
    IAiService aiService,
    IChatMessageService chatMessageService
    ) : Hub<IBoardClient>
{
    [Channel(nameof(JoinBoard))]
    [PublishOperation(typeof(string), Summary = "Подключение к комнате конкретной доски")]
    public async Task JoinBoard(string boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
    }
    
    [Channel(nameof(LeaveBoard))]
    [PublishOperation(typeof(string), Summary = "Отключение от комнаты доски")]
    public async Task LeaveBoard(string boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, boardId);
    }


    [Channel(nameof(SendMessage))]
    [PublishOperation(typeof(SendMessageCommand), Summary = "Отправка сообщения в чат доски")]
    public async Task SendMessage(SendMessageCommand command)
    {
        await aiService.ProcessMessageAsync(command.BoardId, command.Message);
    }
    
    [Channel(nameof(CancelProcessing))]
    [PublishOperation(typeof(void), Summary = "Отмена обработки сообщения")]
    public Task CancelProcessing()
    {
        return Task.CompletedTask;
    }
}