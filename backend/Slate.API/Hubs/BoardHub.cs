using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Saunter.Attributes;
using Slate.Application.Interfaces;

namespace Slate.API.Hubs;

[AsyncApi]
[Authorize]
public class BoardHub : Hub<IBoardClient>
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
}