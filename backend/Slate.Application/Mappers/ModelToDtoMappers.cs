using Slate.Application.Dto.Response;
using Slate.Domain.Entities;

namespace Slate.Application.Mappers;

public static class ModelToDtoMappers
{
    public static BoardDto ToDto(this Board board, bool flat = false)
    {
        return new BoardDto(
            board.Id,
            board.OwnerId,
            board.IsPublic,
            board.Title,
            flat ? [] : board.Members.Select(u => u.ToDto()),
            flat ? [] : board.Columns.Select(c => c.ToDto())
        );
    }

    public static BoardLookupDto ToLookupDto(this Board board)
    {
        return new BoardLookupDto(
            board.Id,
            board.OwnerId,
            board.IsPublic,
            board.Title,
            board.Members.Count,
            board.Columns.Count
        );
    }

    public static ColumnDto ToDto(this Column column, bool flat = false)
    {
        return new ColumnDto(
            column.Id,
            column.BoardId,
            column.Title,
            column.Position,
            flat ? [] : column.Cards.Select(c => c.ToDto())
        );
    }

    public static CardDto ToDto(this Card card)
    {
        return new CardDto(
            card.Id,
            card.ColumnId,
            card.Title,
            card.Description,
            card.Position,
            card.CoverColor,
            card.Checklists.Select(c => c.ToDto())
        );
    }

    public static ChecklistDto ToDto(this Checklist checklist)
    {
        return new ChecklistDto(
            checklist.Id,
            checklist.CardId,
            checklist.Title,
            checklist.Items.Select(i => i.ToDto())
        );
    }

    public static ChecklistItemDto ToDto(this ChecklistItem item)
    {
        return new ChecklistItemDto(
            item.Id,
            item.ChecklistId,
            item.Text,
            item.IsCompleted,
            item.Position
        );
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Username
        );
    }

    public static BoardMemberDto ToDto(this BoardMember boardMember)
    {
        return new BoardMemberDto(boardMember.UserId, boardMember.User.Username, boardMember.AccessLevel);
    }
}