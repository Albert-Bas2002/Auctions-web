namespace Auction.UserAuthService.Contracts.Dtos

{
    public class ErrorDto
    {
        public string Error { get; set; }
        public string Details { get; set; }
        public int Status { get; set; }
    }
}