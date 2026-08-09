using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ICardRepository
{
    Task Add(Card card);
    
    Task<Card?> GetById(Guid cardId);
}