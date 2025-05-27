namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class ChangePasswordDto
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; }
        public string PreviousPassword { get; set; }
    }
}
