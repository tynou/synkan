using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ICardRepository
{
    Task<Card?> GetById(Guid cardId);
}