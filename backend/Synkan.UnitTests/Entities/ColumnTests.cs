using AwesomeAssertions;
using Synkan.Domain.Entities;

namespace Synkan.UnitTests.Entities;

[TestFixture]
public class ColumnTests
{
    [Test]
    public void AddCard_ShouldAssignCorrectPosition()
    {
        var column = new Column(Guid.NewGuid(), "title", 0);
        var card1 = column.AddCard("card 1");
        var card2 = column.AddCard("card 2");

        card1.Position.Should().Be(0);
        card2.Position.Should().Be(1);
        column.Cards.Should().HaveCount(2);
    }
    
    [Test]
    public void RemoveCard_ShouldReorderCardsCorrectly()
    {
        var column = new Column(Guid.NewGuid(), "title", 0);
        var card1 = column.AddCard("card 1");
        var card2 = column.AddCard("card 2");
        var card3 = column.AddCard("card 3");
        
        column.RemoveCard(card2);
        
        card1.Position.Should().Be(0);
        card3.Position.Should().Be(1);
        
        column.Cards.Should().HaveCount(2);
    }
    
    [Test]
    public void MoveCard_ShouldReorderCardsCorrectly()
    {
        var column = new Column(Guid.NewGuid(), "title", 0);
        var card1 = column.AddCard("card 1");
        var card2 = column.AddCard("card 2");
        var card3 = column.AddCard("card 3");
        
        column.MoveCard(card1, 2);
        
        card2.Position.Should().Be(0);
        card3.Position.Should().Be(1);
        card1.Position.Should().Be(2);
    }
}