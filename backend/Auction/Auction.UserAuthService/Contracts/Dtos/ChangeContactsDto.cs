namespace Auction.UserAuthService.Contracts.Dtos
{
    public class ChangeContactsDto
    {
        public Guid UserId { get; set; }
        public string NewContacts { get; set; }
    }
}
