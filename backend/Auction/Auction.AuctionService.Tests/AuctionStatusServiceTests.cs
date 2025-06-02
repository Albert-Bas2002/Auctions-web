using Auction.AuctionService.Application.Services;
using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class AuctionStatusServiceTests
{
    private readonly Mock<IAuctionDetailsRepository> _detailsRepo;
    private readonly Mock<IAuctionStatusRepository> _statusRepo;
    private readonly Mock<IBidRepository> _bidRepo;
    private readonly Mock<IUserCategoryService> _userCategoryService;
    private readonly AuctionStatusService _service;

    public AuctionStatusServiceTests()
    {
        _detailsRepo = new Mock<IAuctionDetailsRepository>();
        _statusRepo = new Mock<IAuctionStatusRepository>();
        _bidRepo = new Mock<IBidRepository>();
        _userCategoryService = new Mock<IUserCategoryService>();

        _service = new AuctionStatusService(
            _detailsRepo.Object,
            _userCategoryService.Object,
            _statusRepo.Object,
            _bidRepo.Object
        );
    }

    [Theory]
    [InlineData("Moderator")]
    [InlineData("Creator")]
    public async Task CloseAuction_SetsCorrectStatus(string userType)
    {
        var auctionId = Guid.NewGuid();
        var details = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), "Title", "Desc", 100);
        var status = AuctionStatus.Create(auctionId);

        _detailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(details);
        _statusRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(status);

        await _service.CloseAuction(auctionId, userType);

        _detailsRepo.Verify(r => r.Update(It.IsAny<AuctionDetails>()), Times.Once);
        _statusRepo.Verify(r => r.Update(It.IsAny<AuctionStatus>()), Times.Once);
        _bidRepo.Verify(r => r.DeleteExceptMaxBidByAuctionId(auctionId), Times.Once);
    }

    [Fact]
    public async Task GetWinnerOrCreatorId_ReturnsCorrectContactDto()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var winnerId = Guid.NewGuid();

        var details = AuctionDetails.Create(auctionId, creatorId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), "Title", "Desc", 100);
        var status = AuctionStatus.Create(auctionId);
        status.SetWinner(winnerId);

        _detailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(details);
        _statusRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(status);
        _userCategoryService.Setup(s => s.GetUserCategoryForAuction(userId, auctionId)).ReturnsAsync("Creator");

        var result = await _service.GetWinnerOrCreatorId(auctionId, userId);

        Assert.Equal(winnerId, result.ContactId);
    }

    [Fact]
    public async Task AuctionDealComplete_UpdatesStatusCorrectly()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var details = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), "Title", "Desc", 100);
        details.Close();

        var status = AuctionStatus.Create(auctionId);
        status.SetWinner(Guid.NewGuid());

        _detailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(details);
        _statusRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(status);
        _userCategoryService.Setup(s => s.GetUserCategoryForAuction(userId, auctionId)).ReturnsAsync("Creator");

        await _service.AuctionDealComplete(auctionId, userId);

        _statusRepo.Verify(r => r.Update(It.IsAny<AuctionStatus>()), Times.Once);
    }

    [Fact]
    public async Task GetAuctionStatus_ReturnsCorrectStatus()
    {
        var auctionId = Guid.NewGuid();
        var details = AuctionDetails.Create(auctionId, Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "Title", "Desc", 100);
        var status = AuctionStatus.Create(auctionId);

        _detailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(details);
        _statusRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(status);

        var result = await _service.GetAuctionStatus(auctionId);

        Assert.Equal("Active", result);
    }

    [Fact]
    public async Task AuctionCheckProcess_ClosesAndSetsWinnerCorrectly()
    {
        var auctionId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var details = AuctionDetails.Create(auctionId, creatorId, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), "Title", "Desc", 100);
        var bid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 150, DateTime.UtcNow);
        var status = AuctionStatus.Create(auctionId);

        _detailsRepo.Setup(r => r.GetAll(true)).ReturnsAsync(new List<AuctionDetails> { details });
        _bidRepo.Setup(r => r.GetMaxByAuctionId(auctionId)).ReturnsAsync(bid);
        _statusRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(status);

        await _service.AuctionCheckProcess();

        _detailsRepo.Verify(r => r.Update(details), Times.Once);
        _statusRepo.Verify(r => r.Update(status), Times.Once);
        _bidRepo.Verify(r => r.DeleteExceptMaxBidByAuctionId(auctionId), Times.Once);
    }
}