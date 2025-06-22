using System.Data;
using Auction.AuctionService.Core.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Auction.AuctionService.Core.Abstractions
{
    public interface IBidRepository
    {
        Task Add(Bid bid);
        Task DeleteAuctionsBids(Guid auctionId);
        Task DeleteBiddersBids(Guid auctionId, Guid bidCreatorId);
        Task DeleteExceptMaxBidByAuctionId(Guid auctionId);
        Task DeleteForUserExceptMaxBid(Guid auctionId, Guid bidCreatorId);
        Task<List<Bid>> GetAllByAuctionId(Guid auctionId);
        Task<List<Bid>> GetAllByUserId(Guid userId);
        Task<Bid?> GetMaxByAuctionId(Guid auctionId);
        Task<List<Bid>> GetMaxByAuctionIds(List<Guid> auctionIds);
        Task<bool> UserHasBidOnAuction(Guid userId, Guid auctionId);
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);      
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}