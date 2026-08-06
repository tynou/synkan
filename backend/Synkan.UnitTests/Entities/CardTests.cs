using AwesomeAssertions;
using Synkan.Domain.Entities;

namespace Synkan.UnitTests.Entities;

[TestFixture]
public class CardTests
{
    [Test]
    public void AssignLabel_WhenLabelBelongsToDifferentBoard_ShouldThrowException()
    {
        var board1Id = Guid.NewGuid();
        var board2Id = Guid.NewGuid();
        
        var column = new Column(board1Id, "to do", 0);
        var card = new Card(column.Id, board1Id, "task 1", 0);
        
        var invalidLabel = new Label(board2Id, "bug", "#FF0000");
        
        var action = () => card.AssignLabel(invalidLabel);
        
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AssignLabel_WhenLabelIsCorrect_ShouldAddLabelToCard()
    {
        var boardId = Guid.NewGuid();
        var column = new Column(boardId, "to do", 0);
        var card = new Card(column.Id, boardId, "task 1", 0);
        var label = new Label(boardId, "feature", "#00FF00");
        
        card.AssignLabel(label);
        
        card.Labels.Should().ContainSingle(l => l.Id == label.Id);
    }
}