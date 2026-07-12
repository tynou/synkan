using Synkan.Domain.Enums;

namespace Synkan.Domain.Entities;

public class BoardAiSettings
{
    public Guid BoardId { get; private set; }
    public string ApiKey { get; private set; } = string.Empty;
    public AiProvider Provider { get; private set; }
    public string Model { get; private set; } = string.Empty;
    
    private BoardAiSettings() { }

    public BoardAiSettings(Guid boardId, string apiKey, AiProvider provider, string model)
    {
        BoardId = boardId;
        ApiKey = apiKey;
        Provider = provider;
        Model = model;
    }
    
    public BoardAiSettings(Guid boardId)
    {
        BoardId = boardId;
    }

    public void UpdateSettings(string apiKey, AiProvider provider, string model)
    {
        ApiKey = apiKey;
        Provider = provider;
        Model = model;
    }
}