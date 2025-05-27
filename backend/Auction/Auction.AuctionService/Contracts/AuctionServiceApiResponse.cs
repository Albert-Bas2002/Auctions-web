namespace Auction.AuctionService.Contracts
{
    public class AuctionServiceApiResponse<T>
    {
        public T? Data { get; set; }
        public ErrorDto? Error { get; set; }
    }
}
