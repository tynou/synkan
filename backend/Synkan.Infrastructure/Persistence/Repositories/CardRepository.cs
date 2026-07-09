using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

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
            .Include(c => c.Column)
                .ThenInclude(col => col.Cards.OrderBy(card => card.Position))
            .Include(c => c.Checklists)
                .ThenInclude(cl => cl.Items.OrderBy(i => i.Position))
            .Include(c => c.Labels)
            .FirstOrDefaultAsync(b => b.Id == cardId);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}