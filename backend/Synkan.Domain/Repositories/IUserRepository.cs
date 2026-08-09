using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IUserRepository
{
    Task Add(User user);
    
    Task<User?> GetByUsernameAsync(string username);
    
    Task<List<User>> GetAllAsync(string username);
    
    Task<User?> GetByIdAsync(Guid id);
}