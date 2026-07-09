namespace Synkan.Application.Services;

public class TornadoPromptBuilder
{
    private const string DefaultSystemPrompt = """
You are an AI Project Manager for Synkan, a collaborative Kanban board system.
You are provided with the full structure of the current board.
You can manage columns and cards using your tools.
Use multiple tools in one response when it makes sense. Call them in parallel.
Answer any questions provided by the user.
Profanity and sensitive language usage permitted.
Don't use markdown.
Respond in the same language as the user. Be concise.
""";
    
    public async Task<string> CreateSystemInstructions()
    {
        var promptBase = await LoadPromptBaseAsync();

        return promptBase;
    }
    
    private async Task<string> LoadPromptBaseAsync()
    {
        return DefaultSystemPrompt;
    }
}