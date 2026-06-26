using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    
    Task<User?> GetByUsernameAsync(string username);
    
    Task<List<User>> GetAllAsync(string username);
    
    Task<User?> GetByIdAsync(Guid id);
}