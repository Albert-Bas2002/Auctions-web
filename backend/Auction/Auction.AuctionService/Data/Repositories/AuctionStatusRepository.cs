using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Entities;
using Auction.UserService.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Auction.AuctionService.Data.Repositories
{
  
    public class AuctionStatusRepository : IAuctionStatusRepository
    {
        private readonly AuctionDbContext _context;

        public AuctionStatusRepository(AuctionDbContext context)
        {
            _context = context;
        }

        public async Task<AuctionStatus> GetByAuctionId(Guid auctionId)
        {
            var entity = await _context.AuctionsStatus
             .AsNoTracking()
                .FirstOrDefaultAsync(r => r.AuctionId == auctionId);

            if (entity == null)
                throw new KeyNotFoundException($"Auction does not exist.");

            var result = AuctionStatus.Create(auctionId);

            if (entity.IsCloseByCreator) result.CloseByCreator();
            if (entity.IsCloseByModerator) result.CloseByModerator();
            if (entity.HasAuctionWinner) result.SetWinner(entity.AuctionWinnerId);
            if (entity.IsDealCompletedByAuctionWinner) result.CompleteDealByWinner();
            if (entity.IsDealCompletedByAuctionCreator) result.CompleteDealByCreator();

            return result;
        }
        public async Task Add(AuctionStatus auctionStatus)
        {
            var existingEntity = await _context.AuctionsStatus
                .FirstOrDefaultAsync(r => r.AuctionId == auctionStatus.AuctionId);

            if (existingEntity != null)
                throw new InvalidOperationException($"Auction already exists.");

            var entity = new AuctionStatusEntity
            {
                AuctionId = auctionStatus.AuctionId,
                IsCloseByCreator = auctionStatus.IsCloseByCreator,
                IsCloseByModerator = auctionStatus.IsCloseByModerator,
                HasAuctionWinner = auctionStatus.HasAuctionWinner,
                AuctionWinnerId = auctionStatus.AuctionWinnerId,
                IsDealCompletedByAuctionWinner = auctionStatus.IsDealCompletedByAuctionWinner,
                IsDealCompletedByAuctionCreator = auctionStatus.IsDealCompletedByAuctionCreator,
                IsCompletelyFinished = auctionStatus.IsCompletelyFinished
            };

            _context.AuctionsStatus.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(AuctionStatus result)
        {
            var statusEntity = await _context.AuctionsStatus
                .FirstOrDefaultAsync(r => r.AuctionId == result.AuctionId);

            if (statusEntity == null)
                throw new InvalidOperationException("Auction not found for update");

            statusEntity.IsCloseByCreator = result.IsCloseByCreator;
            statusEntity.IsCloseByModerator = result.IsCloseByModerator;
            statusEntity.HasAuctionWinner = result.HasAuctionWinner;
            statusEntity.AuctionWinnerId = result.AuctionWinnerId;
            statusEntity.IsDealCompletedByAuctionWinner = result.IsDealCompletedByAuctionWinner;
            statusEntity.IsDealCompletedByAuctionCreator = result.IsDealCompletedByAuctionCreator;
            statusEntity.IsCompletelyFinished = result.IsCompletelyFinished;

            _context.AuctionsStatus.Update(statusEntity);
            await _context.SaveChangesAsync();
        }
        public async Task<List<AuctionStatus>> GetAllByWinnerId(Guid winnerId)
        {
            var statusEntities = await _context.AuctionsStatus
                .Where(r => r.AuctionWinnerId == winnerId)
                .ToListAsync();

            var result = new List<AuctionStatus>();

            foreach (var entity in statusEntities)
            {
                var status = AuctionStatus.Create(entity.AuctionId);

                if (entity.IsCloseByCreator) status.CloseByCreator();
                if (entity.IsCloseByModerator) status.CloseByModerator();
                if (entity.HasAuctionWinner) status.SetWinner(entity.AuctionWinnerId);
                if (entity.IsDealCompletedByAuctionWinner) status.CompleteDealByWinner();
                if (entity.IsDealCompletedByAuctionCreator) status.CompleteDealByCreator();

                result.Add(status);
            }

            return result;
        }


    }






}

