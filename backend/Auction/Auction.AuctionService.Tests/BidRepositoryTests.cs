using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;
using Auction.AuctionService.Data.Repositories;
using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class BidRepositoryTests
{
    private readonly DbContextOptions<AuctionDbContext> _dbContextOptions;

    public BidRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_SavesBidSuccessfully()
    {
        using var context = new AuctionDbContext(_dbContextOptions);
        var repo = new BidRepository(context);
        var bid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100, DateTime.UtcNow);

        await repo.Add(bid);

        var saved = await context.Bids.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal(bid.BidId, saved.BidId);
    }

    [Fact]
    public async Task Add_ThrowsIfBidExists()
    {
        var bidId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);
        context.Bids.Add(new BidEntity { BidId = bidId, AuctionId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        var bid = Bid.Create(bidId, Guid.NewGuid(), Guid.NewGuid(), 200, DateTime.UtcNow);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.Add(bid));
    }

    [Fact]
    public async Task GetAllByAuctionId_ReturnsSortedBids()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.Bids.AddRange(
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = Guid.NewGuid(), Value = 100, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = Guid.NewGuid(), Value = 200, CreationTime = DateTime.UtcNow }
        );

        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        var bids = await repo.GetAllByAuctionId(auctionId);

        Assert.Equal(2, bids.Count);
        Assert.True(bids[0].GetValue() > bids[1].GetValue());
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsOnlyUserBids()
    {
        var userId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.Bids.AddRange(
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = Guid.NewGuid(), BidCreatorId = userId, Value = 300, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = Guid.NewGuid(), BidCreatorId = Guid.NewGuid(), Value = 150, CreationTime = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        var bids = await repo.GetAllByUserId(userId);

        Assert.Single(bids);
        Assert.Equal(userId, bids.First().BidCreatorId);
    }

    [Fact]
    public async Task GetMaxByAuctionIds_ReturnsMaxPerAuction()
    {
        var auction1 = Guid.NewGuid();
        var auction2 = Guid.NewGuid();

        using var context = new AuctionDbContext(_dbContextOptions);
        context.Bids.AddRange(
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auction1, BidCreatorId = Guid.NewGuid(), Value = 100, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auction1, BidCreatorId = Guid.NewGuid(), Value = 300, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auction2, BidCreatorId = Guid.NewGuid(), Value = 250, CreationTime = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        var result = await repo.GetMaxByAuctionIds(new List<Guid> { auction1, auction2 });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.GetValue() == 300);
        Assert.Contains(result, b => b.GetValue() == 250);
    }

    [Fact]
    public async Task DeleteBiddersBids_RemovesOnlyUserBids()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.Bids.AddRange(
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = userId, Value = 100, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = Guid.NewGuid(), Value = 200, CreationTime = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        await repo.DeleteBiddersBids(auctionId, userId);

        Assert.Single(context.Bids);
        Assert.DoesNotContain(context.Bids, b => b.BidCreatorId == userId);
    }

    [Fact]
    public async Task DeleteExceptMaxBidByAuctionId_RemovesLowerBids()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.Bids.AddRange(
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = Guid.NewGuid(), Value = 100, CreationTime = DateTime.UtcNow },
            new BidEntity { BidId = Guid.NewGuid(), AuctionId = auctionId, BidCreatorId = Guid.NewGuid(), Value = 200, CreationTime = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        await repo.DeleteExceptMaxBidByAuctionId(auctionId);

        Assert.Single(context.Bids);
        Assert.Equal(200, context.Bids.First().Value);
    }

    [Fact]
    public async Task UserHasBidOnAuction_ReturnsTrueIfExists()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        using var context = new AuctionDbContext(_dbContextOptions);

        context.Bids.Add(new BidEntity
        {
            BidId = Guid.NewGuid(),
            AuctionId = auctionId,
            BidCreatorId = userId,
            Value = 100,
            CreationTime = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new BidRepository(context);
        var result = await repo.UserHasBidOnAuction(userId, auctionId);

        Assert.True(result);
    }

    [Fact]
    public async Task UserHasBidOnAuction_ReturnsFalseIfNotExists()
    {
        var repo = new BidRepository(new AuctionDbContext(_dbContextOptions));
        var result = await repo.UserHasBidOnAuction(Guid.NewGuid(), Guid.NewGuid());
        Assert.False(result);
    }
}
