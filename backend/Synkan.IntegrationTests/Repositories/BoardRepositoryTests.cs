using AwesomeAssertions;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;
using Synkan.Infrastructure.Persistence;
using Synkan.Infrastructure.Persistence.Repositories;

namespace Synkan.IntegrationTests.Repositories;

public class BoardRepositoryTests : BaseRepositoryTests
{
    private IBoardRepository CreateRepository(AppDbContext context)
    {
        return new BoardRepository(context);
    }

    [Test]
    public async Task GetByIdAsync_WhenBoardExists_ShouldLoadEntireAggregate()
    {
        var user =  await SeedUserAsync();
        var board = await SeedBoardAsync(user.Id);

        await using var context = CreateDbContext();
        var repository = CreateRepository(context);
        var retrievedBoard = await repository.GetById(board.Id);
        
        retrievedBoard.Should().NotBeNull();
    }

    private async Task<User> SeedUserAsync()
    {
        var user = new User("test_user", "password_hash");
        
        await using var context = CreateDbContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    private async Task<Board> SeedBoardAsync(Guid userId)
    {
        var board = new Board(
            userId,
            true,
            "test board"
        );

        var col1 = board.AddColumn("column 1");
        var col2 = board.AddColumn("column 2");
        var col3 = board.AddColumn("column 3");
        
        var card1 = col1.AddCard("card 1");
        var card2 = col1.AddCard("card 2");
        var card3 = col1.AddCard("card 3");
        
        await using var context = CreateDbContext();
        context.Boards.Add(board);
        await context.SaveChangesAsync();

        return board;
    }
}