using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAll(string username);
}