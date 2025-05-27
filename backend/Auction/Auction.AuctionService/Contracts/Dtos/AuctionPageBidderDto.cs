namespace Auction.AuctionService.Contracts.Dtos
{
    public class AuctionPageBidderDto
    {
        public Guid AuctionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Reserve { get; set; }
        public int CurrentBid { get; set; }
        public int BiddersBid { get; set; }

    }
}
