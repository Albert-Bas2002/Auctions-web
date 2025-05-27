namespace Auction.ApiGateway.Contracts
{
    public class ErrorDto
    {
        public string Error { get; set; }
        public string Details { get; set; }
        public int Status { get; set; }
    }
}
