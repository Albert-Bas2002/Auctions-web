using Auction.AuctionService.Application.Services;
using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class AuctionGetDetailsServiceTests
{
    private readonly Mock<IAuctionDetailsRepository> _detailsRepoMock;
    private readonly Mock<IAuctionStatusRepository> _statusRepoMock;
    private readonly Mock<IBidRepository> _bidRepoMock;
    private readonly AuctionGetDetailsService _service;

    public AuctionGetDetailsServiceTests()
    {
        _detailsRepoMock = new Mock<IAuctionDetailsRepository>();
        _statusRepoMock = new Mock<IAuctionStatusRepository>();
        _bidRepoMock = new Mock<IBidRepository>();

        _service = new AuctionGetDetailsService(
            _detailsRepoMock.Object,
            _statusRepoMock.Object,
            _bidRepoMock.Object
        );
    }

    [Fact]
    public async Task GetAll_ReturnsSortedAuctions()
    {
        var expected = new List<AuctionDetails> { CreateDummyAuction() };
        _detailsRepoMock.Setup(r => r.GetAllSorted("price", true, 1)).ReturnsAsync(expected);

        var result = await _service.GetAll("price", true, 1);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetById_ReturnsAuction()
    {
        var auctionId = Guid.NewGuid();
        var expected = CreateDummyAuction(auctionId);
        _detailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(expected);

        var result = await _service.GetById(auctionId);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetByIds_ReturnsListOfAuctions()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var expected = ids.Select(id => CreateDummyAuction(id)).ToList();
        _detailsRepoMock.Setup(r => r.GetByAuctionIds(ids)).ReturnsAsync(expected);

        var result = await _service.GetByIds(ids);

        Assert.Equal(expected.Count, result.Count);
    }

    [Fact]
    public async Task GetByCreatorId_ReturnsCreatorsAuctions()
    {
        var creatorId = Guid.NewGuid();
        var expected = new List<AuctionDetails> { CreateDummyAuction() };
        _detailsRepoMock.Setup(r => r.GetAllByCreatorId(creatorId, true)).ReturnsAsync(expected);

        var result = await _service.GetByCreatorId(creatorId, true);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetByWinnerId_ReturnsWonAuctions()
    {
        var winnerId = Guid.NewGuid();
        var auctionStatusList = new List<AuctionStatus>
        {
            AuctionStatus.Create(Guid.NewGuid())
        };
        var detailsList = auctionStatusList.Select(s => CreateDummyAuction(s.AuctionId)).ToList();

        _statusRepoMock.Setup(r => r.GetAllByWinnerId(winnerId)).ReturnsAsync(auctionStatusList);
        _detailsRepoMock.Setup(r => r.GetByAuctionIds(It.IsAny<List<Guid>>())).ReturnsAsync(detailsList);

        var result = await _service.GetByWinnerId(winnerId);

        Assert.Equal(detailsList, result);
    }

    [Fact]
    public async Task GetByBiddersId_ReturnsOnlyActiveAuctions()
    {
        var userId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();

        var bids = new List<Bid>
        {
            Bid.Create(Guid.NewGuid(), userId, auctionId, 100, DateTime.UtcNow)
        };

        var activeAuction = CreateDummyAuction(auctionId, true);
        var inactiveAuction = CreateDummyAuction(Guid.NewGuid(), false);
        var detailsList = new List<AuctionDetails> { activeAuction, inactiveAuction };

        _bidRepoMock.Setup(r => r.GetAllByUserId(userId)).ReturnsAsync(bids);
        _detailsRepoMock.Setup(r => r.GetByAuctionIds(It.IsAny<List<Guid>>())).ReturnsAsync(detailsList);

        var result = await _service.GetByBiddersId(userId);

        Assert.Single(result);
        Assert.Contains(result, a => a.IsActive);
    }

    [Fact]
    public async Task GetAuctionCount_ReturnsCountDto()
    {
        var expected = new AuctionCountDto { Count = 5 };
        _detailsRepoMock.Setup(r => r.GetAuctionCount(true)).ReturnsAsync(expected);

        var result = await _service.GetAuctionCount(true);

        Assert.Equal(expected.Count, result.Count);
    }

    private AuctionDetails CreateDummyAuction(Guid? id = null, bool isActive = true)
    {
        var auction = AuctionDetails.Create(
            id ?? Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(5),
            "Title",
            "Desc",
            100
        );
        if (!isActive) auction.Close();
        return auction;
    }
}
