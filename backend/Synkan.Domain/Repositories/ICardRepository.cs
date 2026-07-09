using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ICardRepository
{
    Task Create(Card card);
    
    Task Delete(Guid cardId);
    
    Task<Card?> GetById(Guid cardId);
    
    Task SaveChangesAsync();
}