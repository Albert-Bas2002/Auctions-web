namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts
{
    public class ChangePasswordRequest
    {
        public string NewPassword { get; set; }
        public string PreviousPassword { get; set; }
    }
}
