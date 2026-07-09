using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using LlmTornado.ChatFunctions;
using LlmTornado.Common;
using LlmTornado.Infra;
using Synkan.Application.Interfaces;
using Synkan.Domain.Exceptions;

namespace Synkan.Application.Services;

public class TornadoToolsService
{
    private readonly List<Tool> _tools = [];
    
    private readonly Dictionary<string, Func<FunctionCall, Task<string>>> functionCalls = new();
    
    public IReadOnlyList<Tool> Tools => _tools.AsReadOnly();

    public TornadoToolsService(IAiToolsService toolsService)
    {
        RegisterTool(toolsService.CreateColumn);
        RegisterTool(toolsService.UpdateColumnTitle);
        RegisterTool(toolsService.MoveColumn);
        RegisterTool(toolsService.DeleteColumn);
        
        RegisterTool(toolsService.CreateCard);
        RegisterTool(toolsService.UpdateCardContent);
        RegisterTool(toolsService.UpdateCardCover);
        RegisterTool(toolsService.MoveCard);
        RegisterTool(toolsService.DeleteCard);
        
        RegisterTool(toolsService.CreateChecklist);
        RegisterTool(toolsService.DeleteChecklist);
        RegisterTool(toolsService.CreateChecklistItem);
        RegisterTool(toolsService.DeleteChecklistItem);
        RegisterTool(toolsService.ToggleChecklistItem);
    }
    
    public async ValueTask HandleToolCalls(List<FunctionCall> calls)
    {
        Console.WriteLine($"NEED TO USE {calls.Count} TOOLS");
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

        var methodDescriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
        if (methodDescriptionAttribute is null)
            throw new InvalidOperationException($"Delegate description is missing for {method.Name}");
        var methodDescription = methodDescriptionAttribute.Description;

        var parameters = method.GetParameters();
        var toolParameters = parameters.Select(CreateParameter).ToList();

        var tool = new Tool(toolParameters, methodName, methodDescription);
        _tools.Add(tool);

        var executor = CreateExecutor(methodDelegate);

        functionCalls[methodName] = executor;
    }

    private static ToolParam CreateParameter(ParameterInfo parameter)
    {
        var parameterName = parameter.Name;
        if (parameterName is null)
            throw new InvalidOperationException($"Delegate parameter name is missing for {parameter.Member.Name}");
        var parameterDescriptionAttribute = parameter.GetCustomAttribute<DescriptionAttribute>();
        if (parameterDescriptionAttribute is null)
            throw new InvalidOperationException($"Delegate parameter description is missing for {parameter.Member.Name}");
        var parameterDescription = parameterDescriptionAttribute.Description;
        var parameterType = MapTypeToToolParamType(parameter.ParameterType);

        return new ToolParam(parameterName, parameterDescription, parameterType);
    }

    private static Func<FunctionCall, Task<string>> CreateExecutor(Delegate function)
    {
        var parameters = function.Method.GetParameters();

        return async (call) =>
        {
            try
            {
                var arguments = BindArguments(call, parameters);
                var result = function.DynamicInvoke(arguments);
                return await NormalizeResultAsync(result);
            }
            catch
            {
                return "Tool failed";
            }
        };
    }
    
    private static object?[] BindArguments(FunctionCall call, IReadOnlyList<ParameterInfo> parameters)
    {
        var values = call.GetArguments();
        var arguments = new object?[parameters.Count];

        for (var i = 0; i < parameters.Count; i++)
            arguments[i] = BindArgument(parameters[i], values);

        return arguments;
    }

    private static object? BindArgument(ParameterInfo parameter, IReadOnlyDictionary<string, object?> values)
    {
        var parameterName = parameter.Name;
        if (parameterName is null)
            throw new InvalidOperationException($"Delegate parameter name is missing for {parameter.Member.Name}");

        if (!values.TryGetValue(parameterName, out var rawValue) || rawValue is null)
            throw new InvalidOperationException($"Delegate argument for {parameterName} parameter missing");

        return ConvertArgument(rawValue, parameter.ParameterType);
    }
    
    private static async Task<string> NormalizeResultAsync(object? invocationResult)
    {
        return invocationResult switch
        {
            Task<string> task => await task,
            null => $"Tool returned no result.",
            _ => $"Tool returned unsupported result type '{invocationResult.GetType().FullName}'"
        };
    }
    
    private static ToolParamAtomicTypes MapTypeToToolParamType(Type parameterType)
    {
        if (parameterType == typeof(string) || parameterType == typeof(Guid)) return ToolParamAtomicTypes.String;
        if (parameterType == typeof(int) || parameterType == typeof(long)) return ToolParamAtomicTypes.Int;
        if (parameterType == typeof(bool)) return ToolParamAtomicTypes.Bool;
        if (parameterType == typeof(double) || parameterType == typeof(float) || parameterType == typeof(decimal)) return ToolParamAtomicTypes.Float;

        return ToolParamAtomicTypes.String;
    }
    
    private static object ConvertArgument(object rawValue, Type targetType)
    {
        if (targetType == typeof(string)) return ConvertToString(rawValue);
        if (targetType == typeof(bool)) return ConvertToBool(rawValue);
        if (targetType == typeof(int)) return ConvertToInteger(rawValue, targetType);

        throw new InvalidOperationException($"Tool parameter type '{targetType.FullName}' is not supported.");
    }
    
    private static string ConvertToString(object rawValue)
    {
        return rawValue switch
        {
            string value => value,
            JsonElement json => json.GetString()
                ?? throw new InvalidOperationException("Tool argument cannot be null."),
            _ => throw new InvalidOperationException("Tool argument must be a string.")
        };
    }

    private static bool ConvertToBool(object rawValue)
    {
        return rawValue switch
        {
            bool value => value,
            JsonElement json => json.GetBoolean(),
            _ => throw new InvalidOperationException("Tool argument must be a bool.")
        };
    }

    private static object ConvertToInteger(object rawValue, Type targetType)
    {
        return rawValue switch
        {
            int value => value,
            JsonElement json => json.GetInt32(),
            _ => throw new InvalidOperationException("Tool argument must be an int.")
        };
    }
}