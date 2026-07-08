using Saunter.Attributes;
using Slate.Application.Dto.Response;
using Slate.Application.Events;

namespace Slate.Application.Interfaces;

public interface IBoardClient
{
    [Channel(nameof(OnCardContentUpdated))]
    [SubscribeOperation(typeof(CardContentUpdatedEvent), Summary = "Событие обновления содержимого карточки")]
    Task OnCardContentUpdated(CardContentUpdatedEvent @event);
    
    [Channel(nameof(OnCardCoverUpdated))]
    [SubscribeOperation(typeof(CardCoverUpdatedEvent), Summary = "Событие обновления обложки карточки")]
    Task OnCardCoverUpdated(CardCoverUpdatedEvent @event);
    
    [Channel(nameof(OnCardMoved))]
    [SubscribeOperation(typeof(CardMovedEvent), Summary = "Событие перемещения карточки")]
    Task OnCardMoved(CardMovedEvent @event);
    
    [Channel(nameof(OnCardDeleted))]
    [SubscribeOperation(typeof(CardDeletedEvent), Summary = "Событие удаления карточки")]
    Task OnCardDeleted(CardDeletedEvent @event);
    
    [Channel(nameof(OnCardCreated))]
    [SubscribeOperation(typeof(CardDto), Summary = "Событие создания новой карточки")]
    Task OnCardCreated(CardDto card);
    
    
    [Channel(nameof(OnColumnTitleUpdated))]
    [SubscribeOperation(typeof(ColumnTitleUpdatedEvent), Summary = "Событие изменения названия колонки")]
    Task OnColumnTitleUpdated(ColumnTitleUpdatedEvent @event);
    
    [Channel(nameof(OnColumnMoved))]
    [SubscribeOperation(typeof(ColumnMovedEvent), Summary = "Событие перемещения колонки")]
    Task OnColumnMoved(ColumnMovedEvent @event);
    
    [Channel(nameof(OnColumnDeleted))]
    [SubscribeOperation(typeof(ColumnDeletedEvent), Summary = "Событие удаления колонки")]
    Task OnColumnDeleted(ColumnDeletedEvent @event);
    
    [Channel(nameof(OnColumnCreated))]
    [SubscribeOperation(typeof(ColumnDto), Summary = "Событие создания новой колонки")]
    Task OnColumnCreated(ColumnDto column);
    
    
    [Channel(nameof(OnBoardTitleUpdated))]
    [SubscribeOperation(typeof(BoardTitleUpdatedEvent), Summary = "Событие изменения названия доски")]
    Task OnBoardTitleUpdated(BoardTitleUpdatedEvent @event);
    
    [Channel(nameof(OnBoardVisibilityChanged))]
    [SubscribeOperation(typeof(BoardVisibilityChangedEvent), Summary = "Событие изменения доступа к доске")]
    Task OnBoardVisibilityChanged(BoardVisibilityChangedEvent @event);
    
    [Channel(nameof(OnBoardDeleted))]
    [SubscribeOperation(typeof(BoardDeletedEvent), Summary = "Событие удаления доски")]
    Task OnBoardDeleted(BoardDeletedEvent @event);
    
    
    [Channel(nameof(OnMessageChunk))]
    [SubscribeOperation(typeof(MessageChunkEvent), Summary = "Событие отправки куска сообщения")]
    Task OnMessageChunk(MessageChunkEvent @event);
    
    [Channel(nameof(OnMessageCompleted))]
    [SubscribeOperation(typeof(MessageCompletedEvent), Summary = "Событие завершения сообщения")]
    Task OnMessageCompleted(MessageCompletedEvent @event);
    
    [Channel(nameof(OnProcessingFailed))]
    [SubscribeOperation(typeof(ProcessingFailedEvent), Summary = "Событие ошибки обработки сообщения")]
    Task OnProcessingFailed(ProcessingFailedEvent @event);
    
    [Channel(nameof(OnProcessingStarted))]
    [SubscribeOperation(typeof(ProcessingStartedEvent), Summary = "Событие начала обработки сообщения")]
    Task OnProcessingStarted(ProcessingStartedEvent @event);
    
    [Channel(nameof(OnProcessingCompleted))]
    [SubscribeOperation(typeof(ProcessingCompletedEvent), Summary = "Событие завершения обработки сообщения")]
    Task OnProcessingCompleted(ProcessingCompletedEvent @event);
}