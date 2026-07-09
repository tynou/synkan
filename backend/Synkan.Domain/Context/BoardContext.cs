namespace Synkan.Domain.Context;

public record BoardContext(
    Guid Id,
    string Title,
    // IEnumerable<MemberContext> Members,
    IEnumerable<ColumnContext> Columns
);