using Synkan.Application.Dto.Response;
using Synkan.Application.Interfaces;
using Synkan.Application.Mappers;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<List<UserDto>> GetAll(string username)
    {
        var users = await userRepository.GetAllAsync(username);
        return users.Select(u => u.ToDto()).ToList();
    }
}