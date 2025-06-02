using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auction.AuctionService.Application.Services;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;
using Moq;
using Xunit;

public class BidServiceTests
{
    private readonly Mock<IBidRepository> _bidRepoMock = new();
    private readonly Mock<IAuctionDetailsRepository> _auctionDetailsRepoMock = new();
    private readonly BidService _bidService;

    public BidServiceTests()
    {
        _bidService = new BidService(_auctionDetailsRepoMock.Object, null!, _bidRepoMock.Object);
    }

    [Fact]
    public async Task GetByAuctionId_ReturnsBids()
    {
        var auctionId = Guid.NewGuid();
        var bids = new List<Bid>
        {
            Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 10, DateTime.UtcNow)
        };

        _bidRepoMock.Setup(r => r.GetAllByAuctionId(auctionId)).ReturnsAsync(bids);

        var result = await _bidService.GetByAuctionId(auctionId);

        Assert.Equal(bids, result);
    }

    [Fact]
    public async Task GetMaxByAuctionId_ReturnsBid()
    {
        var auctionId = Guid.NewGuid();
        var maxBid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 50, DateTime.UtcNow);

        _bidRepoMock.Setup(r => r.GetMaxByAuctionId(auctionId)).ReturnsAsync(maxBid);

        var result = await _bidService.GetMaxByAuctionId(auctionId);

        Assert.Equal(maxBid, result);
    }

    [Fact]
    public async Task GetUserBidForAuction_ReturnsUserBid()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var userBids = new List<Bid>
        {
            Bid.Create(Guid.NewGuid(), userId, auctionId, 20, DateTime.UtcNow),
            Bid.Create(Guid.NewGuid(), userId, Guid.NewGuid(), 15, DateTime.UtcNow)
        };

        _bidRepoMock.Setup(r => r.GetAllByUserId(userId)).ReturnsAsync(userBids);

        var result = await _bidService.GetUserBidForAuction(auctionId, userId);

        Assert.NotNull(result);
        Assert.Equal(auctionId, result!.AuctionId);
        Assert.Equal(userId, result.BidCreatorId);
    }

    [Fact]
    public async Task Create_ThrowsIfAuctionNotActive()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var auctionDetails = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "title", "desc", 0);
        auctionDetails.Close(); // делаем аукцион неактивным

        _auctionDetailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(auctionDetails);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _bidService.Create(auctionId, userId, 10));
        Assert.Equal("Auction is not Active", ex.Message);
    }

    [Fact]
    public async Task Create_ThrowsIfBidderIsAuctionCreator()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var auctionDetails = AuctionDetails.Create(auctionId, userId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "title", "desc", 0);
        _auctionDetailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(auctionDetails);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _bidService.Create(auctionId, userId, 10));
    }

    [Fact]
    public async Task Create_ReturnsFailureIfBidValueNotPositive()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var auctionDetails = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "title", "desc", 0);
        _auctionDetailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(auctionDetails);

        var result = await _bidService.Create(auctionId, userId, 0);

        Assert.True(result.IsFailure);
        Assert.Equal("Bid value must be greater than zero.", result.Error);
    }

    [Fact]
    public async Task Create_Succeeds_IfBidGreaterThanPrevious()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var previousBid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 10, DateTime.UtcNow);

        var auctionDetails = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "title", "desc", 0);

        _bidRepoMock.Setup(r => r.GetMaxByAuctionId(auctionId)).ReturnsAsync(previousBid);
        _auctionDetailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(auctionDetails);

        _bidRepoMock.Setup(r => r.Add(It.IsAny<Bid>())).Returns(Task.CompletedTask);
        _bidRepoMock.Setup(r => r.DeleteForUserExceptMaxBid(auctionId, userId)).Returns(Task.CompletedTask);

        var result = await _bidService.Create(auctionId, userId, 15);

        Assert.True(result.IsSuccess);
        _bidRepoMock.Verify(r => r.Add(It.IsAny<Bid>()), Times.Once);
        _bidRepoMock.Verify(r => r.DeleteForUserExceptMaxBid(auctionId, userId), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsFailure_IfBidNotGreaterThanPrevious()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var previousBid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 10, DateTime.UtcNow);

        var auctionDetails = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "title", "desc", 0);

        _bidRepoMock.Setup(r => r.GetMaxByAuctionId(auctionId)).ReturnsAsync(previousBid);
        _auctionDetailsRepoMock.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(auctionDetails);

        var result = await _bidService.Create(auctionId, userId, 5);

        Assert.True(result.IsFailure);
        Assert.Equal("New bid must be greater than the previous bid.", result.Error);
    }

    [Fact]
    public async Task Delete_CallsRepository()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _bidRepoMock.Setup(r => r.DeleteBiddersBids(auctionId, userId)).Returns(Task.CompletedTask);

        await _bidService.Delete(auctionId, userId);

        _bidRepoMock.Verify(r => r.DeleteBiddersBids(auctionId, userId), Times.Once);
    }
}
