using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task Add(User user)
    {
        context.Users.Add(user);
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