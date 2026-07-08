using System.Text;
using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.ChatFunctions;
using LlmTornado.Code;
using LlmTornado.Common;
using LlmTornado.Infra;
using Microsoft.Extensions.Options;
using Slate.Application.Common;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Enums;
using Slate.Domain.Repositories;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ChatMessage = Slate.Domain.Entities.ChatMessage;

namespace Slate.Application.Services;

public class TornadoAiService(
    IOptions<TornadoAiOptions> options,
    IChatMessageService chatMessageService,
    IChatMessageRepository chatMessageRepository,
    IBoardRepository boardRepository,
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
        
        // poolside/laguna-m.1:free
        // nvidia/nemotron-3-ultra-550b-a55b:free
        var conversation = new LlmTornadoConversation(api.Chat.CreateConversation(new ChatRequest
        {
            Model = new ChatModel("poolside/laguna-m.1:free", LLmProviders.OpenRouter),
            Tools = [
                new Tool(
                    [
                        new ToolParam(
                            "columnId",
                            "The Id of a column in which to create a card",
                            ToolParamAtomicTypes.String
                            ),
                        new ToolParam(
                            "title",
                            "The title the created card should have",
                            ToolParamAtomicTypes.String
                        ),
                    ],
                    "CreateCard",
                    "Create a new card"
                    )
            ],
            Temperature = temperature,
        }));
        
        var systemPrompt = await promptBuilder.CreateSystemInstructions();
        
        var board = await boardRepository.GetById(message.BoardId);
        var boardContext = board.ToContext();
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var boardContextYaml = serializer.Serialize(boardContext);
        Console.WriteLine(boardContextYaml);
        
        conversation.PrependSystemMessage($"{boardContextYaml}\n\n{systemPrompt}");
        
        conversation.AddUserMessage(message.Content);

        for (var i = 0; i < maxTurns; i++)
        {
            var aiMessageId = Guid.NewGuid();
            var (response, callsCount) = await conversation.StreamResponseAsync(async tokens =>
                {
                    await chatMessageService.SendMessageChunkAsync(message.BoardId, aiMessageId, tokens);
                },
                async calls =>
                {
                    foreach (var call in calls)
                    {
                        Console.WriteLine($"calling a tool {call.Name}");
                        call.Result = new FunctionResult(call, Guid.NewGuid().ToString(), true);
                    }
                });

            if (!string.IsNullOrWhiteSpace(response))
                await chatMessageRepository.AddAsync(new ChatMessage(aiMessageId, message.BoardId, ChatMessageRole.Ai, response));
        
            await chatMessageService.SendMessageCompletedAsync(message.BoardId, aiMessageId);

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
        Func<List<FunctionCall>, ValueTask> toolCallsHandler
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
            null
            );

        return (messageBuffer.ToString(), currentToolCalls);
    }

    private void CleanBuffer()
    {
        messageBuffer.Clear();
        currentToolCalls = 0;
    }
}