using AwesomeAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using OpenTelemetry.Trace;
using Synkan.Application.Dto.Response;
using Synkan.Application.Events;
using Synkan.Application.Hubs;
using Synkan.Application.Interfaces;
using Synkan.Application.Services;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;
using Synkan.Domain.Exceptions;
using Synkan.Domain.Repositories;

namespace Synkan.UnitTests.Services;

[TestFixture]
public class CardServiceTests
{
    private IColumnRepository _columnRepository;
    private ICardRepository _cardRepository;
    private IBoardMemberRepository _boardMemberRepository;
    private ILabelRepository _labelRepository;
    private IUnitOfWork _unitOfWork;
    private Tracer _tracer;
    private ICurrentUserService _currentUserService;
    private IHubContext<BoardHub, IBoardClient> _hubContext;
    private IBoardClient _boardClient;
    
    private CardService _sut;

    [SetUp]
    public void SetUp()
    {
        _columnRepository = Substitute.For<IColumnRepository>();
        _cardRepository = Substitute.For<ICardRepository>();
        _boardMemberRepository = Substitute.For<IBoardMemberRepository>();
        _labelRepository = Substitute.For<ILabelRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tracer = TracerProvider.Default.GetTracer("TestTracer");
        _currentUserService = Substitute.For<ICurrentUserService>();
        _hubContext = Substitute.For<IHubContext<BoardHub, IBoardClient>>();
        _boardClient = Substitute.For<IBoardClient>();
        
        _hubContext.Clients.Group(Arg.Any<string>()).Returns(_boardClient);

        _sut = new CardService(
            _columnRepository,
            _cardRepository,
            _boardMemberRepository,
            _labelRepository,
            _unitOfWork,
            _tracer,
            _currentUserService,
            _hubContext
        );
    }
    
    private (Guid UserId, Guid BoardId, Column Column, BoardMember Member) CreateTestContext(AccessLevel accessLevel = AccessLevel.Member)
    {
        var userId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var column = new Column(boardId, "test column", 0);
        var member = new BoardMember(boardId, userId, accessLevel);

        _currentUserService.UserId.Returns(userId);
        _boardMemberRepository.GetAsync(boardId, userId).Returns(member);

        return (userId, boardId, column, member);
    }

    private Card CreateTestCard(Column column, string title = "test card")
    {
        var card = column.AddCard(title);
        
        typeof(Card)
            .GetProperty(nameof(Card.Column))?
            .SetValue(card, column);

        return card;
    }

    [Test]
    public async Task Create_WhenUserHasNoAccess_ShouldThrowUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var column = new Column(boardId, "to do", 0);

        _currentUserService.UserId.Returns(userId);
        _columnRepository.GetById(columnId).Returns(column);
        _boardMemberRepository.GetAsync(boardId, userId).ReturnsNull();
        
        var action = () => _sut.Create(columnId, "card");
        
        await action.Should().ThrowAsync<UnauthorizedException>();
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public async Task Create_WhenUserHasAccess_ShouldCreateCardAndNotifyClients()
    {
        var ctx = CreateTestContext();
        _columnRepository.GetById(ctx.Column.Id).Returns(ctx.Column);
        
        var cardId = await _sut.Create(ctx.Column.Id, "new card");
        
        cardId.Should().NotBeEmpty();
        ctx.Column.Cards.Should().ContainSingle(c => c.Title == "new card");
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardCreated(Arg.Is<CardDto>(dto => dto.Title == "new card"));
    }
    
    [Test]
    public async Task UpdateContent_WhenUserHasAccess_ShouldUpdateCardContentAndNotifyClients()
    {
        var ctx = CreateTestContext();
        var card = CreateTestCard(ctx.Column);
        _cardRepository.GetById(card.Id).Returns(card);
        
        await _sut.UpdateContent(card.Id, "new title", "new description");
        
        card.Title.Should().Be("new title");
        card.Description.Should().Be("new description");
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardContentUpdated(Arg.Is<CardContentUpdatedEvent>(e =>
                e.Title == "new title" && e.Description == "new description"));
    }
    
    [Test]
    public async Task UpdateColor_WhenUserHasAccess_ShouldUpdateCardColorAndNotifyClients()
    {
        var ctx = CreateTestContext();
        var card = CreateTestCard(ctx.Column);
        _cardRepository.GetById(card.Id).Returns(card);
        
        await _sut.UpdateCover(card.Id, "#FF0000");
        
        card.CoverColor.Should().Be("#FF0000");
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardCoverUpdated(Arg.Is<CardCoverUpdatedEvent>(e => e.Color == "#FF0000"));
    }
    
    [Test]
    public async Task Move_WhenUserHasAccess_SameColumn_ShouldMoveCardAndNotifyClients()
    {
        var ctx = CreateTestContext();
        var card1 = CreateTestCard(ctx.Column, "card 1");
        CreateTestCard(ctx.Column, "card 2");
        CreateTestCard(ctx.Column, "card 3");
        
        _columnRepository.GetById(ctx.Column.Id).Returns(ctx.Column);
        _cardRepository.GetById(card1.Id).Returns(card1);
        
        await _sut.Move(card1.Id, ctx.Column.Id, 2);
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardMoved(Arg.Is<CardMovedEvent>(e => e.Position == 2));
    }
    
    [Test]
    public async Task Move_WhenUserHasAccess_DifferentColumn_ShouldMoveCardAndNotifyClients()
    {
        var ctx = CreateTestContext();
        var newColumn = new Column(ctx.BoardId, "test column 2", 1);
        
        var card1 = CreateTestCard(ctx.Column, "card 1");
        CreateTestCard(ctx.Column, "card 2");
        
        _columnRepository.GetById(newColumn.Id).Returns(newColumn);
        _cardRepository.GetById(card1.Id).Returns(card1);
        
        await _sut.Move(card1.Id, newColumn.Id, 0);
        
        ctx.Column.Cards.Should().HaveCount(1);
        newColumn.Cards.Should().HaveCount(1);
        card1.ColumnId.Should().Be(newColumn.Id);
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardMoved(Arg.Is<CardMovedEvent>(e => e.Position == 0));
    }
    
    [Test]
    public async Task AssignLabel_WhenCardAndLabelExist_ShouldAssignAndNotify()
    {
        var ctx = CreateTestContext();
        
        var card = CreateTestCard(ctx.Column);
        var label = new Label(ctx.BoardId, "bug", "#FF0000");

        _cardRepository.GetById(card.Id).Returns(card);
        _labelRepository.GetById(label.Id).Returns(label);

        await _sut.AssignLabel(card.Id, label.Id);

        card.Labels.Should().ContainSingle(l => l.Id == label.Id);
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardLabelAssigned(Arg.Is<CardLabelAssignedEvent>(e => e.CardId == card.Id && e.LabelId == label.Id));
    }
    
    [Test]
    public async Task RemoveLabel_WhenCardHasLabel_ShouldRemoveAndNotify()
    {
        var ctx = CreateTestContext();
        
        var card = CreateTestCard(ctx.Column);
        var label = new Label(ctx.BoardId, "bug", "#FF0000");
        
        card.AssignLabel(label);

        _cardRepository.GetById(card.Id).Returns(card);

        await _sut.RemoveLabel(card.Id, label.Id);

        card.Labels.Should().BeEmpty();
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardLabelRemoved(Arg.Is<CardLabelRemovedEvent>(e => e.CardId == card.Id && e.LabelId == label.Id));
    }
    
    [Test]
    public async Task Delete_WhenUserHasAccess_ShouldRemoveCardFromColumnAndNotify()
    {
        var ctx = CreateTestContext();
        var card = CreateTestCard(ctx.Column);

        _cardRepository.GetById(card.Id).Returns(card);

        await _sut.Delete(card.Id);

        ctx.Column.Cards.Should().BeEmpty();
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        await _boardClient.Received(1).OnCardDeleted(Arg.Is<CardDeletedEvent>(e => e.CardId == card.Id));
    }
    
    [Test]
    public async Task GetById_WhenUserHasAccess_ShouldReturnMappedDto()
    {
        var ctx = CreateTestContext(AccessLevel.Viewer);
        var card = CreateTestCard(ctx.Column, "title");

        _cardRepository.GetById(card.Id).Returns(card);

        var result = await _sut.GetById(card.Id);

        result.Should().NotBeNull();
        result.Title.Should().Be("title");
    }
}