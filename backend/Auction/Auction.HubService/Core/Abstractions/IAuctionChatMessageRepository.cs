using Auction.HubService.Core.Models;
using Auction.HubService.Data.Entities;

namespace Auction.HubService.Core.Abstractions
{
    public interface IAuctionChatMessageRepository
    {
        Task<List<AuctionChatMessage>> GetMessagesForAuctionChat(Guid auctionId);
        Task SaveMessage(AuctionChatMessage message);

    }
}