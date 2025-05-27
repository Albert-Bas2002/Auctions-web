using System.Net.Http;
using Auction.HubService.Core.Abstractions;
using Auction.HubService.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Auction.HubService.Application.Hubs
{
    public class AuctionHub : Hub<IClient>
    {
        private readonly IAuctionHubService _auctionHubService;
        private readonly ILogger<AuctionHub> _logger;
        public AuctionHub(IAuctionHubService auctionHubService, ILogger<AuctionHub> logger)
        {
            _auctionHubService = auctionHubService;  
            _logger = logger;
        }

        public async Task JoinAuctionGroup(Guid auctionId)
        {
            try
            {
                var isAuctionActive = await _auctionHubService.IsAuctionActive(auctionId);
                if (isAuctionActive)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString());

                    var history = await _auctionHubService.GetHistoryForAuctionChat(auctionId);

                    foreach (var message in history)
                    {
                        await Clients.Caller.ReceiveMessage(message.UserCategoryForAuction, message.Message, message.Timestamp);
                    }
                }
                else return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinAuctionChat for auction {AuctionId}", auctionId);

                await Clients.Caller.ReceiveMessage("Server", "Internal Server Error. Please try again later", DateTime.UtcNow);
            }
        }

        public async Task SendMessageAuctionChat(Guid auctionId,string message)
        {
            try
            {
                var senderIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                var senderId = string.IsNullOrEmpty(senderIdClaim) ? Guid.Empty : Guid.Parse(senderIdClaim);

                var isAuctionActive = await _auctionHubService.IsAuctionActive(auctionId);
                if (!isAuctionActive)
                    return;

                var userCategory = await _auctionHubService.GetUserCategoryForAuction(senderId, auctionId);

                await Clients
                    .Group(auctionId.ToString())
                    .ReceiveMessage(userCategory, message, DateTime.UtcNow);

                var auctionChatMessage = AuctionChatMessage.Create(
                    Guid.NewGuid(),
                    auctionId,
                    senderId,
                    DateTime.UtcNow,
                    userCategory,
                    message
                );

                await _auctionHubService.SaveMessage(auctionChatMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessageAuctionChat for auction");

                await Clients.Caller.ReceiveMessage("Server", "Internal Server Error. Please try again later", DateTime.UtcNow);
            }
        }

        public async Task<string> CreateBid(Guid auctionId, int amountBid)
        {
            try
            {
                var senderIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (senderIdClaim == null)
                {
                    return "Unauthorized: No user ID claim found";
                }

                var isAuctionActive = await _auctionHubService.IsAuctionActive(auctionId);
                if (!isAuctionActive)
                {
                    return "Auction is not active";
                }

                var createBidResult = await _auctionHubService.CreateBid(auctionId, Guid.Parse(senderIdClaim), amountBid);
                if (createBidResult == "Success")
                {
                    await Clients
                        .Group(auctionId.ToString())
                        .ReceiveBidUpdate();


                    return "Bid sent successfully";
                }
                else
                {
                    return createBidResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendBidToAuctionGroup for auction {AuctionId}", auctionId);
                return "Server error while sending bid";
            }
        }
        public async Task<string> DeleteBid(Guid auctionId)
        {
            try
            {
                var senderIdClaim = Context.User?.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (senderIdClaim == null)
                {
                    return "Unauthorized: No user ID claim found";
                }

                var isAuctionActive = await _auctionHubService.IsAuctionActive(auctionId);
                if (!isAuctionActive)
                {
                    return "Auction is not active";
                }

                var deleteBidResult = await _auctionHubService.DeleteBid(auctionId, Guid.Parse(senderIdClaim));
                if (deleteBidResult == "Success")
                {
                    await Clients
                        .Group(auctionId.ToString())
                        .ReceiveBidUpdate();

                    return "Bid deleted successfully";
                }
                else
                {
                    return deleteBidResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteBidFromAuctionGroup for auction {AuctionId}", auctionId);
                return "Server error while deleting bid";
            }
        }

        public async Task LeaveAuctionGroup(Guid auctionId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LeaveAuctionChat for auction {AuctionId}", auctionId);
            }
        }
    }
}
