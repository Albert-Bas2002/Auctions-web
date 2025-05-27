namespace Auction.HubService.Core.Models
{
    public class AuctionChatMessage
    {
        public const int MAX_MESSAGE_LENGHT = 200;
        public Guid MessageId { get; private set; }

        public Guid AuctionId { get; private set; }
        public Guid SenderId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string UserCategoryForAuction { get; private set; }
        public string Message { get; private set; }

        private AuctionChatMessage(Guid messageId,Guid auctionId, Guid senderId, DateTime timestamp, string userCategory, string message)
        {
            MessageId = messageId;
            AuctionId = auctionId;
            SenderId = senderId;
            Timestamp = timestamp;
            UserCategoryForAuction = userCategory;
            Message = message;
        }

        public static AuctionChatMessage Create(Guid messageId,Guid auctionId, Guid senderId, DateTime timestamp, string userCategoryForAuction, string message)
        {
            if (messageId == Guid.Empty)
                throw new ArgumentException("MessageId cannot be empty", nameof(messageId));

            if (auctionId == Guid.Empty)
                throw new ArgumentException("AuctionId cannot be empty", nameof(auctionId));


            if (string.IsNullOrWhiteSpace(userCategoryForAuction))
                throw new ArgumentException($"UserCategory must be non-empty .", nameof(userCategoryForAuction));

            if (string.IsNullOrWhiteSpace(message) || message.Length > MAX_MESSAGE_LENGHT)
                throw new ArgumentException($"Message must be non-empty and less than {MAX_MESSAGE_LENGHT} characters.", nameof(message));

            return new AuctionChatMessage(messageId, auctionId, senderId, timestamp, userCategoryForAuction, message);
        }
    }
}
