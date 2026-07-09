namespace Synkan.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}