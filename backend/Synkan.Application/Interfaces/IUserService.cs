using Synkan.Application.Dto.Response;

namespace Synkan.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAll(string username);
}