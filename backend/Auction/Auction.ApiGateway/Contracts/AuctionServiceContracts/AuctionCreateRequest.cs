namespace Auction.ApiGateway.Contracts.AuctionServiceContracts
{
    public class AuctionCreateRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int AuctionDurationInDays { get; set; }
        public int Reserve { get; set; }
    }
}
