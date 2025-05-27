namespace Auction.UserAuthService.Contracts.Dtos
{
    public class ChangeUserNameDto
    {
        public Guid UserId { get; set; }
        public string NewUserName { get; set; }
    }
}
