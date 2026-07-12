using Synkan.Domain.Enums;

namespace Synkan.Application.Dto.Request;

public record UpdateBoardAiSettingsRequest(
    string ApiKey,
    AiProvider Provider,
    string Model
);