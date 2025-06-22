using System.Data;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;

using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Auction.AuctionService.Data.Repositories
{
    public class BidRepository : IBidRepository
    {
        private readonly AuctionDbContext _context;
        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }


        public BidRepository(AuctionDbContext context)
        {
            _context = context;
        }
        public async Task Add(Bid bid)
        {
            var existingEntity = await _context.Bids
                 .FirstOrDefaultAsync(r => r.AuctionId == bid.BidId);

            if (existingEntity != null)
                throw new InvalidOperationException($"Bid already exists.");

            var BidEntity = new BidEntity
            {
                BidId = bid.BidId,
                BidCreatorId = bid.BidCreatorId,
                AuctionId = bid.AuctionId,
                Value = bid.GetValue(),
                CreationTime = bid.CreationTime,

            };

            _context.Bids.Add(BidEntity);
            await _context.SaveChangesAsync();
        }
        public async Task<List<Bid>> GetAllByAuctionId(Guid auctionId)
        {
            var bidEntities = await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Value)
                .ToListAsync();

            return bidEntities
                .Select(e => Bid.Create(e.BidId, e.BidCreatorId, e.AuctionId, e.Value, e.CreationTime))
                .ToList();
        }
        public async Task<List<Bid>> GetAllByUserId(Guid userId)
        {
            var bidEntities = await _context.Bids
                .Where(b => b.BidCreatorId == userId)
                .OrderByDescending(b => b.Value)
                .ToListAsync();

            return bidEntities
                .Select(e => Bid.Create(e.BidId, e.BidCreatorId, e.AuctionId, e.Value, e.CreationTime))
                .ToList();
        }

        public async Task<List<Bid>> GetMaxByAuctionIds(List<Guid> auctionIds)
        {
            var bids = await _context.Bids
                .Where(b => auctionIds.Contains(b.AuctionId))
                .GroupBy(b => b.AuctionId)
                .Select(g => g
                    .OrderByDescending(b => b.Value)
                    .FirstOrDefault())
                .ToListAsync();

            return bids
                .Where(b => b != null)
                .Select(b => Bid.Create(b.BidId, b.BidCreatorId, b.AuctionId, b.Value, b.CreationTime))
                .ToList();
        }
        public async Task<Bid?> GetMaxByAuctionId(Guid auctionId)
        {
            return (await GetMaxByAuctionIds(new List<Guid> { auctionId })).FirstOrDefault();
        }


        public async Task DeleteBiddersBids(Guid auctionId, Guid bidCreatorId)
        {
            var allBids = await _context.Bids
                            .Where(b => b.AuctionId == auctionId)
                            .Where(b => b.BidCreatorId == bidCreatorId)
                            .ToListAsync();

            _context.Bids.RemoveRange(allBids);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAuctionsBids(Guid auctionId)
        {
            var allBids = await _context.Bids
                            .Where(b => b.AuctionId == auctionId)
                            .ToListAsync();

            _context.Bids.RemoveRange(allBids);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteExceptMaxBidByAuctionId(Guid auctionId)
        {
            var allBids = await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Value)
                .ToListAsync();

            if (allBids.Count == 0)
            {
                return;
            }

            var maxBid = allBids.First();
            var bidsToDelete = allBids.Skip(1).ToList();
            _context.Bids.RemoveRange(bidsToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteForUserExceptMaxBid(Guid auctionId, Guid bidCreatorId)
        {
            var allBids = await _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .Where(b => b.BidCreatorId == bidCreatorId)
                .OrderByDescending(b => b.Value)
                .ToListAsync();

            if (allBids.Count == 0)
            {
                return;
            }

            var maxBid = allBids.First();
            var bidsToDelete = allBids.Skip(1).ToList();
            _context.Bids.RemoveRange(bidsToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserHasBidOnAuction(Guid userId, Guid auctionId)
        {
            return await _context.Bids
                .AnyAsync(b => b.BidCreatorId == userId && b.AuctionId == auctionId);
        }

    }
}
