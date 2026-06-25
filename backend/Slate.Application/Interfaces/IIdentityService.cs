using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IIdentityService
{
    Task<string> Register(string username, string password);
    
    Task<string> Login(string username, string password);
    
    Task<UserDto> GetMe(Guid userId);
}