using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Saunter.Attributes;
using Synkan.Application.Events;
using Synkan.Application.Interfaces;
using Synkan.Application.Services;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Hubs;

[AsyncApi]
[Authorize]
public class BoardHub(
    IAiService aiService,
    ISettingsService settingsService,
    IChatMessageService chatMessageService,
    IChatMessageRepository chatMessageRepository,
    IUnitOfWork unitOfWork,
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
            await unitOfWork.SaveChangesAsync();
            
            await chatMessageService.SendMessageSentAsync(command.BoardId, message);

            var settings = await settingsService.GetOrCreateAsync(command.BoardId);
        
            await aiService.ProcessMessageAsync(command.BoardId, command.Message, settings, ct);

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
        Console.WriteLine("Trying to cancel the operation...");
        operationService.CancelOperation(command.BoardId.ToString());
        return Task.CompletedTask;
    }
}