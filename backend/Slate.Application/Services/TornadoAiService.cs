using System.Text;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.ChatFunctions;
using LlmTornado.Code;
using LlmTornado.Common;
using Microsoft.Extensions.Options;
using Slate.Application.Common;
using Slate.Application.Interfaces;
using Slate.Domain.Enums;
using Slate.Domain.Repositories;
using ChatMessage = Slate.Domain.Entities.ChatMessage;

namespace Slate.Application.Services;

public class TornadoAiService(
    IOptions<TornadoAiOptions> options,
    IChatMessageService chatMessageService,
    IChatMessageRepository chatMessageRepository,
    TornadoPromptBuilder promptBuilder
    ) : IAiService
{
    private readonly double temperature = options.Value.Temperature;
    private readonly int maxTurns = options.Value.MaxTurns;
    
    public async Task ProcessMessageAsync(ChatMessage message)
    {
        var api = new TornadoApi(
            LLmProviders.OpenRouter,
            "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"
            );
        
        var conversation = new LlmTornadoConversation(api.Chat.CreateConversation(new ChatRequest
        {
            Model = new ChatModel("poolside/laguna-m.1:free", LLmProviders.OpenRouter),
            Temperature = temperature,
        }));
        
        var systemPrompt = await promptBuilder.CreateSystemInstructions();
        conversation.PrependSystemMessage(systemPrompt);
        conversation.AddUserMessage(message.Content);

        var aiMessageId = Guid.NewGuid();
        var result = await conversation.StreamResponseAsync(async tokens =>
        {
            await chatMessageService.SendMessageChunkAsync(message.BoardId, aiMessageId, tokens);
        });

        await chatMessageRepository.AddAsync(new ChatMessage(aiMessageId, message.BoardId, ChatMessageRole.Ai, result));
        
        await chatMessageService.SendMessageCompletedAsync(message.BoardId, aiMessageId);
    }
}

public sealed class LlmTornadoConversation(Conversation conversation)
{
    private readonly StringBuilder messageBuffer = new();

    public void PrependSystemMessage(string instructions)
    {
        conversation.PrependSystemMessage(instructions);
    }

    // public void AddMessages(IEnumerable<TornadoChatMessage> messages)
    // {
    //     conversation.AddMessage(messages);
    // }

    public void AddUserMessage(string message)
    {
        conversation.AddUserMessage(message);
    }

    public async Task<string> StreamResponseAsync(Func<string, ValueTask> tokensHandler)
    {
        CleanBuffer();
        await conversation.StreamResponseRich(async tokens =>
            {
                messageBuffer.Append(tokens);
                if (string.IsNullOrEmpty(tokens))
                    return;
                
                await tokensHandler(tokens);
            },
            null,
            null
            );

        return messageBuffer.ToString();
    }

    private void CleanBuffer()
    {
        messageBuffer.Clear();
    }
}