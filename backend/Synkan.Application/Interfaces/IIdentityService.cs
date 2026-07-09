using Synkan.Application.Dto.Response;

namespace Synkan.Application.Interfaces;

public interface IIdentityService
{
    Task<string> Register(string username, string password);
    
    Task<string> Login(string username, string password);
    
    Task<UserDto> GetMe(Guid userId);
}