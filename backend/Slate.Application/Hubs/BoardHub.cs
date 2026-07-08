using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Saunter.Attributes;
using Slate.Application.Events;
using Slate.Application.Interfaces;
using Slate.Application.Services;
using Slate.Domain.Entities;
using Slate.Domain.Enums;
using Slate.Domain.Repositories;

namespace Slate.Application.Hubs;

[AsyncApi]
[Authorize]
public class BoardHub(
    IAiService aiService,
    IChatMessageService chatMessageService,
    IChatMessageRepository chatMessageRepository,
    TornadoPromptBuilder promptBuilder,
    IProcessingOperationService operationService,
    ILogger<BoardHub> logger
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
        var (operationId, ctSource) = operationService.BeginOperation(command.BoardId.ToString());
        var ct = ctSource.Token;

        try
        {
            await chatMessageService.SendProcessingStartedAsync(command.BoardId);
            
            var message = new ChatMessage(
                Guid.NewGuid(),
                command.BoardId,
                ChatMessageRole.User,
                command.Message
            );
        
            await chatMessageRepository.AddAsync(message);
        
            await aiService.ProcessMessageAsync(command.BoardId, command.Message, ct);

            await chatMessageService.SendProcessingCompletedAsync(command.BoardId);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            logger.LogInformation("Chat processing cancelled for board {BoardId}", command.BoardId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Chat processing failed for board {BoardId}", command.BoardId);
            await chatMessageService.SendProcessingFailedAsync(command.BoardId);
        }
        finally
        {
            operationService.CompleteOperation(command.BoardId.ToString(), operationId);
        }
    }
    
    [Channel(nameof(CancelProcessing))]
    [PublishOperation(typeof(CancelProcessingCommand), Summary = "Отмена обработки сообщения")]
    public Task CancelProcessing(CancelProcessingCommand command)
    {
        operationService.CancelOperation(command.BoardId.ToString());
        return Task.CompletedTask;
    }
}