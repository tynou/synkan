using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Username == username);
    }

    public async Task<List<User>> GetAllAsync(string username)
    {
        return await context.Users
            .Where(u => EF.Functions.Like(u.Username, $"%{username}%"))
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}