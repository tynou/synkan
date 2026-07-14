namespace Synkan.Application.Events;

public record CardLabelRemovedEvent(Guid CardId, Guid LabelId);