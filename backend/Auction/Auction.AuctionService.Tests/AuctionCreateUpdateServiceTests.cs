using Auction.AuctionService.Application.Services;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class AuctionCreateUpdateServiceTests
{
    private readonly Mock<IAuctionDetailsRepository> _mockDetailsRepo;
    private readonly Mock<IAuctionStatusRepository> _mockStatusRepo;
    private readonly AuctionCreateUpdateService _service;

    public AuctionCreateUpdateServiceTests()
    {
        _mockDetailsRepo = new Mock<IAuctionDetailsRepository>();
        _mockStatusRepo = new Mock<IAuctionStatusRepository>();
        _service = new AuctionCreateUpdateService(_mockDetailsRepo.Object, _mockStatusRepo.Object);
    }

    [Fact]
    public async Task Create_CallsAddOnBothRepositories()
    {

        var creatorId = Guid.NewGuid();
        var title = "Test Title";
        var description = "Test Description";
        var reserve = 100;
        var durationDays = 3;

        await _service.Create(creatorId, durationDays, title, description, reserve);


        _mockDetailsRepo.Verify(repo => repo.Add(It.IsAny<AuctionDetails>()), Times.Once);
        _mockStatusRepo.Verify(repo => repo.Add(It.IsAny<AuctionStatus>()), Times.Once);
    }

    [Fact]
    public async Task Update_ThrowsException_WhenAuctionIdIsEmpty()
    {

        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.Update(Guid.Empty, userId, "title", "desc"));
    }

    [Fact]
    public async Task Update_ReturnsFailure_WhenUserIsNotCreator()
    {

        var auctionId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var mockAuction = AuctionDetails.Create(auctionId, creatorId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "Old", "Old Desc", 100);

        _mockDetailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(mockAuction);


        var result = await _service.Update(auctionId, anotherUserId, "New Title", "New Desc");


        Assert.True(result.IsFailure);
        Assert.Equal("Auction can only be updated by the creator", result.Error);
        _mockDetailsRepo.Verify(r => r.Update(It.IsAny<AuctionDetails>()), Times.Never);
    }

    [Fact]
    public async Task Update_ReturnsSuccess_WhenUserIsCreator()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockAuction = AuctionDetails.Create(auctionId, userId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), "Old", "Old Desc", 100);

        _mockDetailsRepo.Setup(r => r.GetByAuctionId(auctionId)).ReturnsAsync(mockAuction);

        
        var result = await _service.Update(auctionId, userId, "Updated Title", "Updated Desc");

        Assert.True(result.IsSuccess);
        _mockDetailsRepo.Verify(r => r.Update(It.Is<AuctionDetails>(a =>
            a.GetTitle() == "Updated Title" && a.GetDescription() == "Updated Desc")), Times.Once);
    }
}
