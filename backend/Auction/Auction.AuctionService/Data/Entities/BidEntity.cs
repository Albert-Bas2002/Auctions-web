using Auction.Core.Models.ValueObjects;

namespace Auction.AuctionService.Data.Entities
{
    public class BidEntity
    {
        public Guid BidId { get; set; }
        public Guid BidCreatorId { get; set; }
        public Guid AuctionId { get; set; }
        public int Value { get; set; }
        public DateTime CreationTime { get; set; }

    }
}
