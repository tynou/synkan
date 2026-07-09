namespace Synkan.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}