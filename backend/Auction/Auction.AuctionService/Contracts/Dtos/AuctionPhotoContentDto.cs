namespace Auction.AuctionService.Contracts.Dtos
{
    public class AuctionPhotoContentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }

}
