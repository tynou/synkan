using Saunter.Attributes;
using Slate.Application.Dto.Response;
using Slate.Application.Events;

namespace Slate.Application.Interfaces;

public interface IBoardClient
{
    [Channel(nameof(OnCardMoved))]
    [SubscribeOperation(typeof(CardMovedEvent), Summary = "Событие перемещения карточки")]
    Task OnCardMoved(CardMovedEvent @event);
    
    [Channel(nameof(OnCardCreated))]
    [SubscribeOperation(typeof(CardDto), Summary = "Событие создания новой карточки")]
    Task OnCardCreated(CardDto card);
}