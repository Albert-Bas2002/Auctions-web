namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class ChangeUserNameDto
    {
        public Guid UserId { get; set; }
        public string NewUserName { get; set; }
    }
}
