using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<List<UserDto>> GetAll(string username)
    {
        var users = await userRepository.GetAllAsync(username);
        return users.Select(u => u.ToDto()).ToList();
    }
}