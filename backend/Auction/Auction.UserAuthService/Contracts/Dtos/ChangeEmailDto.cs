namespace Auction.UserAuthService.Contracts.Dtos
{
    public class ChangeEmailDto
    {
        public Guid UserId { get; set; }
        public string NewEmail { get; set; }
    }
}
