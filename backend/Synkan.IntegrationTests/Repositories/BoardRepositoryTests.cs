using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
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
        retrievedBoard.Columns.Should().HaveCount(3);
        retrievedBoard.Columns.First().Cards.Should().HaveCount(3);

        context.ChangeTracker.Entries<Board>().Should().HaveCount(1);
    }

    [Test]
    public async Task Delete_WhenBoardExists_ShouldCascadeDeleteColumnsAndCards()
    {
        var user = await SeedUserAsync();
        var board = await SeedBoardAsync(user.Id);
        
        await using (var contextForDelete = CreateDbContext())
        {
            var repository = CreateRepository(contextForDelete);
            
            await repository.Delete(board.Id);
            await contextForDelete.SaveChangesAsync(); 
        }
        
        await using (var contextForAssert = CreateDbContext())
        {
            var boardExists = await contextForAssert.Boards.AnyAsync(b => b.Id == board.Id);
            boardExists.Should().BeFalse();
            
            var columnsExist = await contextForAssert.Columns.AnyAsync(c => c.BoardId == board.Id);
            columnsExist.Should().BeFalse();

            var cardsExist = await contextForAssert.Cards.AnyAsync(c => c.BoardId == board.Id);
            cardsExist.Should().BeFalse();
        }
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
        
        await using var context = CreateDbContext();
        context.Boards.Add(board);

        var col1 = board.AddColumn("column 1");
        var col2 = board.AddColumn("column 2");
        var col3 = board.AddColumn("column 3");
        
        var card1 = col1.AddCard("card 1");
        var card2 = col1.AddCard("card 2");
        var card3 = col1.AddCard("card 3");
        
        await context.SaveChangesAsync();

        return board;
    }
}