namespace Slate.Application.Interfaces;

public interface IAiService
{
    Task ProcessMessageAsync(Guid boardId, string userPrompt);
}