using Auction.HubService.Core.Abstractions;
using Auction.HubService.Core.Models;
using Auction.HubService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.HubService.Data.Repositories
{
    public class AuctionChatMessageRepository : IAuctionChatMessageRepository
    {
        private readonly AuctionChatDbContext _context;

        public AuctionChatMessageRepository(AuctionChatDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessage(AuctionChatMessage message)
        {
            var auctionChatMessageEntity = new AuctionChatMessageEntity
            {
                MessageId = message.MessageId,
                AuctionId = message.AuctionId,
                Message = message.Message,
                UserCategoryForAuction = message.UserCategoryForAuction,
                Timestamp = message.Timestamp,
                SenderId = message.SenderId

            };
            _context.AuctionChatMessages.Add(auctionChatMessageEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuctionChatMessage>> GetMessagesForAuctionChat(Guid auctionId)
        {
            var auctionChatMessageEntities = await _context.AuctionChatMessages
                .Where(m => m.AuctionId == auctionId)
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .OrderBy(m => m.Timestamp) 
                .ToListAsync();

            var auctionChatMessages = auctionChatMessageEntities
                .Select(entity => AuctionChatMessage.Create(
                    entity.MessageId,
                    entity.AuctionId,
                    entity.SenderId,
                    entity.Timestamp,
                    entity.UserCategoryForAuction,
                    entity.Message))
                .ToList();

            return auctionChatMessages;
        }

    }

}
