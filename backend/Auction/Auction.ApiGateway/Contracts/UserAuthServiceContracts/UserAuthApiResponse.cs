namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts
{
    public class UserAuthApiResponse<T>
    {
        public T? Data { get; set; }
        public ErrorDto? Error { get; set; }
    }
}
