using Synkan.Domain.Enums;

namespace Synkan.Application.Dto.Response;

public record BoardAiSettingsDto(
    string ApiKey,
    AiProvider Provider,
    string Model
);