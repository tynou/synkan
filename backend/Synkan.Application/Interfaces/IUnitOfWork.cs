namespace Synkan.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}