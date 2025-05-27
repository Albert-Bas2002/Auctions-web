namespace Auction.ApiGateway.Contracts.AuctionServiceContracts
{
    public class AuctionServiceApiResponse<T>
    {
        public T? Data { get; set; }
        public ErrorDto? Error { get; set; }
    }
}
