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
            card.Position
        );
    }

    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Username
        );
    }
}