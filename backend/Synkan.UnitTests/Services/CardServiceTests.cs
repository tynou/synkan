using AwesomeAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using OpenTelemetry.Trace;
using Synkan.Application.Dto.Response;
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
        var userId = Guid.NewGuid();
        var columnId = Guid.NewGuid();
        var boardId = Guid.NewGuid();
        var column = new Column(boardId, "to do", 0);
        var member = new BoardMember(boardId, userId, AccessLevel.Member);

        _currentUserService.UserId.Returns(userId);
        _columnRepository.GetById(columnId).Returns(column);
        _boardMemberRepository.GetAsync(boardId, userId).Returns(member);
        
        var cardId = await _sut.Create(columnId, "new card");
        
        cardId.Should().NotBeEmpty();
        column.Cards.Should().ContainSingle(c => c.Title == "new card");
        
        await _unitOfWork.Received(1).SaveChangesAsync();
        
        await _boardClient.Received(1).OnCardCreated(Arg.Is<CardDto>(dto => dto.Title == "new card"));
    }
}