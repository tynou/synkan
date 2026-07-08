namespace Slate.Application.Interfaces;

public interface IAiToolsService
{
    Task<string> CreateCard(string columnId, string title);
}