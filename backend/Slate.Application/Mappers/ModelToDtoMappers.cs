using Slate.Application.Dto.Response;
using Slate.Domain.Entities;

namespace Slate.Application.Mappers;

public static class ModelToDtoMappers
{
    public static BoardDto ToDto(this Board board)
    {
        return new BoardDto(
            board.Id,
            board.OwnerId,
            board.Title,
            board.Columns.Select(c => c.ToDto())
        );
    }

    public static ColumnDto ToDto(this Column column)
    {
        return new ColumnDto();
    }
}