using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface ICardRepository
{
    Task Create(Card card);
    
    Task Delete(Guid cardId);
    
    Task<Card?> GetById(Guid cardId);
    
    Task SaveChangesAsync();
}