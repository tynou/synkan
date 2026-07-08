using System.ComponentModel;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class AiToolsService(
    ICardService cardService,
    ICurrentUserService currentUser
    ) : IAiToolsService
{
    [Description("Creates a new card in the specified column of the board")]
    public async Task<string> CreateCard(
        [Description("The Id of the column")] string columnId,
        [Description("The title of the card")] string title
        )
    {
        Console.WriteLine($"Does this work? {columnId} {title}");
        var result = await cardService.Create(currentUser.UserId, Guid.Parse(columnId), title);
        return result.ToString();
    }
}