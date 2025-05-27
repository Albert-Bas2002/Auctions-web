namespace Auction.ApiGateway.Contracts.ApiGatewayContracts
{
    public class ApiGatewayResponse<T>
    {
        public T? Data { get; set; }
        public ErrorDto? Error { get; set; }
    }
}
