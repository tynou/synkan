using Slate.Domain.Entities;

namespace Slate.Application.Interfaces;

public interface IAiService
{
    Task ProcessMessageAsync(ChatMessage message);
}