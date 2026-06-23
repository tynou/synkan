namespace Slate.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}