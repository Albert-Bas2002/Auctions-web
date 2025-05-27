namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class ChangeEmailDto
    {
        public Guid UserId { get; set; }
        public string NewEmail { get; set; }
    }
}
