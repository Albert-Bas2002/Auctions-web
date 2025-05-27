namespace Auction.AuctionService.Data.Entities
{
    public class AuctionDetailsEntity
    {
        public Guid AuctionId { get; set; }
        public Guid AuctionCreatorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public int Reserve { get; set; }
        public AuctionStatusEntity? AuctionStatus { get; set; }

    }

}
