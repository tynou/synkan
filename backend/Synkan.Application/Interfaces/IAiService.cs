using Synkan.Domain.Entities;

namespace Synkan.Application.Interfaces;

public interface IAiService
{
    Task ProcessMessageAsync(Guid boardId, string content, BoardAiSettings settings, CancellationToken ct);
}