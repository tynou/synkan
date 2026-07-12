using System.Text;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.ChatFunctions;
using Microsoft.Extensions.Options;
using Synkan.Application.Common;
using Synkan.Application.Interfaces;
using Synkan.Application.Mappers;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;
using Synkan.Domain.Exceptions;
using Synkan.Domain.Repositories;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ChatMessage = Synkan.Domain.Entities.ChatMessage;

namespace Synkan.Application.Services;

public class TornadoAiService(
    IOptions<TornadoAiOptions> options,
    IChatMessageService chatMessageService,
    IChatMessageRepository chatMessageRepository,
    IBoardRepository boardRepository,
    IUnitOfWork unitOfWork,
    TornadoPromptBuilder promptBuilder,
    TornadoToolsService toolsService
    ) : IAiService
{
    private readonly double temperature = options.Value.Temperature;
    private readonly int maxTurns = options.Value.MaxTurns;
    
    public async Task ProcessMessageAsync(Guid boardId, string content, BoardAiSettings settings, CancellationToken ct)
    {
        // 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
        // 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
        // var api = new TornadoApi(
        //     LLmProviders.OpenRouter,
        //     "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"
        //     );
        
        var api = new TornadoApi(
            settings.Provider.ToLlmProviders(),
            settings.ApiKey
        );
        
        // gemini-2.5-flash
        // gemini-2.5-flash-lite
        // gemini-2.5-pro
        // gemini-3.1-flash-lite
        // gemini-3.5-flash
        
        // poolside/laguna-m.1:free
        // nvidia/nemotron-3-ultra-550b-a55b:free
        // openrouter/auto
        // tencent/hy3:free
        var conversation = new LlmTornadoConversation(api.Chat.CreateConversation(new ChatRequest
        {
            Model = new ChatModel(settings.Model, settings.Provider.ToLlmProviders()),
            Tools = toolsService.Tools.ToList(),
            Temperature = temperature,
            ParallelToolCalls = true,
        }));
        
        var systemPrompt = await promptBuilder.CreateSystemInstructions();
        
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found");
        var boardContext = board.ToContext();
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var boardContextYaml = serializer.Serialize(boardContext);
        
        conversation.PrependSystemMessage($"Board structure:\n\n{boardContextYaml}\n\n{systemPrompt}");
        
        conversation.AddUserMessage(content);

        for (var i = 0; i < maxTurns; i++)
        {
            var aiMessageId = Guid.NewGuid();
            var (response, callsCount) = await conversation.StreamResponseAsync(async tokens =>
                {
                    await chatMessageService.SendMessageChunkAsync(boardId, aiMessageId, tokens);
                },
                toolsService.HandleToolCalls,
                ct);

            if (!string.IsNullOrWhiteSpace(response))
            {
                await chatMessageRepository.AddAsync(new ChatMessage(aiMessageId, boardId, ChatMessageRole.Ai, response));
                await unitOfWork.SaveChangesAsync();
            }
        
            await chatMessageService.SendMessageCompletedAsync(boardId, aiMessageId);

            if (callsCount == 0)
                break;
        }
    }
}

public sealed class LlmTornadoConversation(Conversation conversation)
{
    private readonly StringBuilder messageBuffer = new();
    private int currentToolCalls;

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

    public async Task<(string response, int toolCalls)> StreamResponseAsync(
        Func<string, ValueTask> tokensHandler,
        Func<List<FunctionCall>, ValueTask> toolCallsHandler,
        CancellationToken ct
        )
    {
        CleanBuffer();
        await conversation.StreamResponseRich(async tokens =>
            {
                messageBuffer.Append(tokens);
                if (string.IsNullOrEmpty(tokens))
                    return;
                
                await tokensHandler(tokens);
            },
            async calls =>
            {
                currentToolCalls += calls.Count;
                await toolCallsHandler(calls);
            },
            null,
            token: ct
            );

        return (messageBuffer.ToString(), currentToolCalls);
    }

    private void CleanBuffer()
    {
        messageBuffer.Clear();
        currentToolCalls = 0;
    }
}