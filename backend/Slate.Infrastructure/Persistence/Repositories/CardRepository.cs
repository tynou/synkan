using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class CardRepository(AppDbContext context) : ICardRepository
{
    public async Task Create(Card card)
    {
        context.Cards.Add(card);
        await context.SaveChangesAsync();
    }

    public async Task Delete(Guid cardId)
    {
        context.Cards.Remove(context.Cards.First(c => c.Id == cardId));
        await context.SaveChangesAsync();
    }

    public async Task<Card?> GetById(Guid cardId)
    {
        return await context.Cards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == cardId);
    }
}