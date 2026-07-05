namespace Slate.Application.Services;

public class TornadoPromptBuilder
{
    private const string DefaultSystemPrompt = """
You are an AI Project Manager for Slate, a collaborative Kanban board system.
You can manage columns and cards using your tools.
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