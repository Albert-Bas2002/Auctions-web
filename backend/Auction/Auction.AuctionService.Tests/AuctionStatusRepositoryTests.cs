using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;
using Auction.AuctionService.Data.Repositories;
using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AuctionStatusRepositoryTests
{
    private readonly DbContextOptions<AuctionDbContext> _dbContextOptions;

    public AuctionStatusRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_SavesAuctionStatusToDatabase()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repo = new AuctionStatusRepository(context);

        var status = AuctionStatus.Create(Guid.NewGuid());

        await repo.Add(status);

        var saved = await context.AuctionsStatus.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal(status.AuctionId, saved.AuctionId);
    }

   

    [Fact]
    public async Task Update_ChangesAndPersistsStatus()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.AuctionsStatus.Add(new AuctionStatusEntity
        {
            AuctionId = auctionId,
            IsCloseByCreator = false
        });

        await context.SaveChangesAsync();

        var repo = new AuctionStatusRepository(context);
        var status = AuctionStatus.Create(auctionId);
        status.CloseByCreator();

        await repo.Update(status);

        var updated = await context.AuctionsStatus.FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        Assert.True(updated.IsCloseByCreator);
    }

    [Fact]
    public async Task GetAllByWinnerId_ReturnsCorrectStatuses()
    {
        var winnerId = Guid.NewGuid();
        var auctionId1 = Guid.NewGuid();
        var auctionId2 = Guid.NewGuid();

        using var context = new AuctionDbContext(_dbContextOptions);

        context.AuctionsStatus.AddRange(
            new AuctionStatusEntity
            {
                AuctionId = auctionId1,
                AuctionWinnerId = winnerId,
                HasAuctionWinner = true
            },
            new AuctionStatusEntity
            {
                AuctionId = auctionId2,
                AuctionWinnerId = winnerId,
                HasAuctionWinner = true
            });

        await context.SaveChangesAsync();

        var repo = new AuctionStatusRepository(context);
        var statuses = await repo.GetAllByWinnerId(winnerId);

        Assert.Equal(2, statuses.Count);
        Assert.All(statuses, s => Assert.Equal(winnerId, s.AuctionWinnerId));
    }

    [Fact]
    public async Task GetByAuctionId_ThrowsIfNotFound()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repo = new AuctionStatusRepository(context);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repo.GetByAuctionId(Guid.NewGuid()));

        Assert.Equal("Auction does not exist.", ex.Message);
    }

    [Fact]
    public async Task Add_ThrowsIfAuctionAlreadyExists()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.AuctionsStatus.Add(new AuctionStatusEntity
        {
            AuctionId = auctionId
        });

        await context.SaveChangesAsync();

        var repo = new AuctionStatusRepository(context);
        var status = AuctionStatus.Create(auctionId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.Add(status));
    }
}
