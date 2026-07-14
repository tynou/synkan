namespace Synkan.Domain.Context;

public record LabelContext(
    Guid Id,
    string Name,
    string Color
);