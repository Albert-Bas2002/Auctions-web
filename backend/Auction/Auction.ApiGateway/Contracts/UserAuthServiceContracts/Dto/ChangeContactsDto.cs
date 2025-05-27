namespace Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto
{
    public class ChangeContactsDto
    {
        public Guid UserId { get; set; }
        public string NewContacts { get; set; }
    }
}
