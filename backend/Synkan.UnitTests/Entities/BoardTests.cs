using AwesomeAssertions;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;

namespace Synkan.UnitTests.Entities;

[TestFixture]
public class BoardTests
{
    [Test]
    public void Constructor_ShouldAddOwnerAsAdminMember()
    {
        var ownerId = Guid.NewGuid();
        
        var board = new Board(ownerId, false, "test board");
        
        board.Members.Should().ContainSingle();
        
        var member = board.Members.First();
        member.UserId.Should().Be(ownerId);
        member.AccessLevel.Should().Be(AccessLevel.Admin);
    }
    
    [Test]
    public void AddColumn_ShouldAssignCorrectPosition()
    {
        var board = new Board(Guid.NewGuid(), false, "board");
        
        var col1 = board.AddColumn("first");
        var col2 = board.AddColumn("second");
        
        col1.Position.Should().Be(0);
        col2.Position.Should().Be(1);
        board.Columns.Should().HaveCount(2);
    }
    
    [Test]
    public void MoveColumn_ShouldReorderColumnsCorrectly()
    {
        var board = new Board(Guid.NewGuid(), false, "board");
        var col1 = board.AddColumn("column 1");
        var col2 = board.AddColumn("column 2");
        var col3 = board.AddColumn("column 3");

        board.MoveColumn(col1.Id, 2);

        col2.Position.Should().Be(0);
        col3.Position.Should().Be(1);
        col1.Position.Should().Be(2);
    }
    
    [Test]
    public void RemoveMember_WhenUserIsOwner_ShouldNotRemoveMember()
    {
        var ownerId = Guid.NewGuid();
        var board = new Board(ownerId, isPublic: false, "Board");
        
        board.RemoveMember(ownerId);
        
        board.Members.Should().ContainSingle(m => m.UserId == ownerId);
    }
}