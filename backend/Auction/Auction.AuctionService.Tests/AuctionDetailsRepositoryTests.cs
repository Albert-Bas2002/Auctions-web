using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data;
using Auction.AuctionService.Data.Repositories;
using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AuctionDetailsRepositoryTests
{
    private DbContextOptions<AuctionDbContext> _dbContextOptions;

    public AuctionDetailsRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AuctionDetails CreateTestAuction()
    {
        return AuctionDetails.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(5),
            "Test Auction",
            "This is a test auction description.",
            100
        );
    }

    [Fact]
    public async Task Add_AddsAuctionToDatabase()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);
        var auction = CreateTestAuction();

        await repository.Add(auction);

        var savedAuction = await context.AuctionsDetails.FirstOrDefaultAsync();
        Assert.NotNull(savedAuction);
        Assert.Equal(auction.AuctionId, savedAuction.AuctionId);
    }

    [Fact]
    public async Task Add_ThrowsException_WhenAuctionAlreadyExists()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);
        var auction = CreateTestAuction();

        await repository.Add(auction);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.Add(auction));
    }

    [Fact]
    public async Task GetByAuctionId_ReturnsCorrectAuction()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);
        var auction = CreateTestAuction();

        await repository.Add(auction);

        var result = await repository.GetByAuctionId(auction.AuctionId);

        Assert.NotNull(result);
        Assert.Equal(auction.AuctionId, result.AuctionId);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyActiveAuctions()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);

        var activeAuction = CreateTestAuction();
        var inactiveAuction = CreateTestAuction();
        inactiveAuction.Close();

        await repository.Add(activeAuction);
        await repository.Add(inactiveAuction);

        await repository.Update(inactiveAuction); 
        var activeAuctions = await repository.GetAll(true);
        var inactiveAuctions = await repository.GetAll(false);

        Assert.Single(activeAuctions);
        Assert.Single(inactiveAuctions);
    }

    [Fact]
    public async Task Update_UpdatesAuctionDetails()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);
        var auction = CreateTestAuction();

        await repository.Add(auction);

        auction.ChangeTitle("Updated Title");
        await repository.Update(auction);

        var updated = await repository.GetByAuctionId(auction.AuctionId);
        Assert.Equal("Updated Title", updated.GetTitle());
    }

    [Fact]
    public async Task GetAuctionCount_ReturnsCorrectCount()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repository = new AuctionDetailsRepository(context);

        var auction = CreateTestAuction();
        await repository.Add(auction);

        var countDto = await repository.GetAuctionCount(true);

        Assert.Equal(1, countDto.Count);
    }
}
