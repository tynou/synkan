using LlmTornado.Code;
using Synkan.Domain.Enums;
using Synkan.Domain.Exceptions;

namespace Synkan.Application.Mappers;

public static class LlmTornadoMappers
{
    public static LLmProviders ToLlmProviders(this AiProvider provider)
    {
        return provider switch
        {
            AiProvider.Unknown => LLmProviders.Unknown,
            AiProvider.OpenAI => LLmProviders.OpenAi,
            AiProvider.OpenRouter => LLmProviders.OpenRouter,
            AiProvider.Gemini => LLmProviders.Google,
            AiProvider.Anthropic => LLmProviders.Anthropic,
            _ => throw new NotFoundException("Provider not found")
        };
    }
}