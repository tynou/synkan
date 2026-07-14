namespace Synkan.Application.Events;

public record CardLabelAssignedEvent(Guid CardId, Guid LabelId);