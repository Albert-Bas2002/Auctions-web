namespace Auction.HubService.Data.Entities
{
    public class AuctionChatMessageEntity
    {
        public Guid MessageId { get; set; }
        public Guid AuctionId { get; set; }
        public Guid SenderId { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserCategoryForAuction { get; set; }
        public string Message { get; set; }

    }
}
