namespace Auction.HubService.Core.Abstractions
{
    public interface IClient
    {
        public Task ReceiveMessage(string userName,string message, DateTime ttimestamp);
        Task ReceiveBidUpdate();


    }
}
