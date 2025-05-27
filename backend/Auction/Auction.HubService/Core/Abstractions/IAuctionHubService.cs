using Auction.HubService.Core.Models;

namespace Auction.HubService.Core.Abstractions
{
    public interface IAuctionHubService
    {
        Task<List<AuctionChatMessage>> GetHistoryForAuctionChat(Guid auctionId);
        Task<string> GetUserCategoryForAuction(Guid userId, Guid auctionId);
        Task<bool> IsAuctionActive(Guid auctionId);
        Task SaveMessage(AuctionChatMessage auctionChatMessage);
        Task<string> CreateBid(Guid auctionId, Guid userId, int bidValue);
        Task<string> DeleteBid(Guid auctionId, Guid userId);
    }
}