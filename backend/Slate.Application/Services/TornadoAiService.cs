using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.ChatFunctions;
using LlmTornado.Code;
using LlmTornado.Common;
using Microsoft.Extensions.Options;
using Slate.Application.Common;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class TornadoAiService(
    IOptions<TornadoAiOptions> options,
    IChatMessageService chatMessageService
    ) : IAiService
{
    private readonly double temperature = options.Value.Temperature;
    private readonly int maxTurns = options.Value.MaxTurns;
    
    public async Task ProcessMessageAsync(Guid boardId, string userPrompt)
    {
        // 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
        var api = new TornadoApi(
            LLmProviders.OpenRouter,
            "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"
            );
        
        var conversation = api.Chat.CreateConversation(new ChatRequest
        {
            Model = new ChatModel("nvidia/nemotron-3-ultra-550b-a55b:free", LLmProviders.OpenRouter),
            Temperature = temperature,
        });
        
        // conversation.AppendSystemMessage(
        //     "You are an AI Project Manager for Slate, a collaborative Kanban board system. " +
        //     "You can manage columns and cards using your tools. " +
        //     "Respond in the same language as the user. Be concise."
        //     );

        conversation.AppendMessage(ChatMessageRoles.User, userPrompt);
        
        await conversation.StreamResponseRich(async tokens =>
            {
                await chatMessageService.SendMessageChunkAsync(boardId, tokens);
            },
        null,
        null);
        
    }
}