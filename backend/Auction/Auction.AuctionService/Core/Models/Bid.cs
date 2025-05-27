using Auction.Core.Models.ValueObjects;

namespace Auction.AuctionService.Core.Models
{
    public class Bid
    {
        public Guid BidId { get; } 
        public Guid BidCreatorId { get; }
        public Guid AuctionId { get; }

        private readonly BidValue _value;
        public DateTime CreationTime { get; }
        private Bid(Guid bidId, Guid bidCreatorId, Guid auctionId, int value, DateTime creationTime)
        {

            BidId  = bidId;
            BidCreatorId = bidCreatorId;
            AuctionId=auctionId;
            CreationTime = creationTime;
            _value = BidValue.Create(value);
        }
        public static Bid Create(Guid bidId,Guid bidCreatorId, Guid auctionId, int value, DateTime creationTime)
        {
            if (value <= 0)
            {
                throw new InvalidOperationException("Bid canonot be negative or null.");
            }

            return new Bid(bidId, bidCreatorId, auctionId, value,creationTime);
        }

        public int GetValue()
        {
            return _value.value;
        }
    }
}
