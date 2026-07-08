using Slate.Domain.Context;
using Slate.Domain.Entities;

namespace Slate.Application.Mappers;

public static class ModelToContextMappers
{
    public static BoardContext ToContext(this Board board)
    {
        return new BoardContext(
            board.Id,
            board.Title,
            board.Columns.Select(c => c.ToContext())
        );
    }

    public static ColumnContext ToContext(this Column column)
    {
        return new ColumnContext(
            column.Id,
            column.Title,
            column.Position,
            column.Cards.Select(c => c.ToContext())
        );
    }

    public static CardContext ToContext(this Card card)
    {
        return new CardContext(
            card.Id,
            card.Title,
            card.Description,
            card.Position,
            card.Checklists.Select(c => c.ToContext())
        );
    }

    public static ChecklistContext ToContext(this Checklist checklist)
    {
        return new ChecklistContext(
            checklist.Id,
            checklist.Title,
            checklist.Items.Select(i => i.ToContext())
        );
    }

    public static ChecklistItemContext ToContext(this ChecklistItem item)
    {
        return new ChecklistItemContext(
            item.Id,
            item.Text,
            item.IsCompleted
        );
    }
}