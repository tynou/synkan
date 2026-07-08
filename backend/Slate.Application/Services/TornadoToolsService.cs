using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using LlmTornado.ChatFunctions;
using LlmTornado.Common;
using LlmTornado.Infra;
using Slate.Application.Interfaces;
using Slate.Domain.Exceptions;

namespace Slate.Application.Services;

public class TornadoToolsService
{
    private readonly List<Tool> _tools = [];
    
    private readonly Dictionary<string, Func<FunctionCall, Task<string>>> functionCalls = new();
    
    public IReadOnlyList<Tool> Tools => _tools.AsReadOnly();

    public TornadoToolsService(IAiToolsService toolsService)
    {
        RegisterTool(toolsService.CreateCard);
    }
    
    public async ValueTask HandleToolCalls(List<FunctionCall> calls)
    {
        foreach (var call in calls)
        {
            Console.WriteLine($"CALLING TOOL: {call.Name}");
            try
            {
                var result = await HandleFunctionCall(call);
                call.Result = new FunctionResult(call, result, true);
            }
            catch (Exception e)
            {
                call.Result = new FunctionResult(call, e.Message, false);
            }
        }
    }

    private async Task<string> HandleFunctionCall(FunctionCall call)
    {
        if (string.IsNullOrWhiteSpace(call.Name) || !functionCalls.TryGetValue(call.Name, out var functionCall))
            throw new InvalidToolCallException($"There is no such tool as: {call.Name}");

        return await functionCall(call);
    }
    
    private void RegisterTool(Delegate methodDelegate)
    {
        var method = methodDelegate.Method;
        var methodName = method.Name;

        var methodDescAttr = method.GetCustomAttribute<DescriptionAttribute>();
        var methodDescription = methodDescAttr?.Description ?? $"Executes {methodName}";

        var parameters = method.GetParameters();
        var toolParams = new List<ToolParam>();

        foreach (var param in parameters)
        {
            var paramDescAttr = param.GetCustomAttribute<DescriptionAttribute>();
            var paramDescription = paramDescAttr?.Description ?? $"The {param.Name} parameter";
            var paramType = MapTypeToToolParamType(param.ParameterType);

            toolParams.Add(new ToolParam(param.Name!, paramDescription, paramType));
        }

        var tool = new Tool(toolParams, methodName, methodDescription);
        _tools.Add(tool);

        Func<FunctionCall, Task<string>> executor = async (FunctionCall call) =>
        {
            var argsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(call.Arguments) 
                           ?? [];

            var invokeArgs = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                if (argsDict.TryGetValue(param.Name!, out var rawValue))
                {
                    var jsonElement = (JsonElement)rawValue;
                    invokeArgs[i] = ConvertJsonElement(jsonElement, param.ParameterType);
                }
                else
                {
                    invokeArgs[i] = param.DefaultValue ?? null!;
                }
            }

            var result = methodDelegate.DynamicInvoke(invokeArgs);

            if (result is Task<string> task)
            {
                return await task;
            }

            return result?.ToString() ?? "Success";
        };

        functionCalls[methodName] = executor;
    }
    
    private static ToolParamAtomicTypes MapTypeToToolParamType(Type type)
    {
        if (type == typeof(string) || type == typeof(Guid)) return ToolParamAtomicTypes.String;
        if (type == typeof(int) || type == typeof(long)) return ToolParamAtomicTypes.Int;
        if (type == typeof(bool)) return ToolParamAtomicTypes.Bool;
        if (type == typeof(double) || type == typeof(float) || type == typeof(decimal)) return ToolParamAtomicTypes.Float;

        return ToolParamAtomicTypes.String;
    }

    private static object ConvertJsonElement(JsonElement element, Type targetType)
    {
        if (targetType == typeof(string)) return element.GetString()!;
        if (targetType == typeof(Guid)) return Guid.Parse(element.GetString()!);
        if (targetType == typeof(int)) return element.GetInt32();
        if (targetType == typeof(bool)) return element.GetBoolean();
        
        return element.ToString();
    }
}