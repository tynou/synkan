namespace Slate.Domain.Context;

public record ColumnContext(
    Guid Id,
    string Title,
    int Position,
    IEnumerable<CardContext> Cards
);