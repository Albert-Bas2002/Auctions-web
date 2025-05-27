namespace Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos
{
    public class AuctionCreateDto
    {
        public string Title { get; set; } 
        public string Description { get; set; }
        public int Reserve { get; set; } 
        public int AuctionDurationInDays { get; set; }
    }
}
