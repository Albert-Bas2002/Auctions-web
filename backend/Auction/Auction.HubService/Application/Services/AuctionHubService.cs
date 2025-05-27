using Auction.HubService.Contracts;
using Auction.HubService.Core.Abstractions;
using Auction.HubService.Core.Models;
using Newtonsoft.Json;

namespace Auction.HubService.Application.Services
{
    public class AuctionHubService : IAuctionHubService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuctionChatMessageRepository _chatMessageRepository;

        public AuctionHubService(IHttpClientFactory httpClientFactory, IAuctionChatMessageRepository chatMessageRepository)
        {
            _httpClient = httpClientFactory.CreateClient("AuctionService");
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<string> GetUserCategoryForAuction(Guid userId, Guid auctionId)
        {
            var response = await _httpClient.GetAsync($"api-auctions/get-category/auction/{auctionId}/user/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Internal Server Error. Please try again later.");
            }

            var categoryResponse = await response.Content.ReadFromJsonAsync<AuctionServiceApiResponse<UserCategoryForAuctionDto>>();

            if (categoryResponse == null)
            {
                throw new Exception("User data is unavailable.");
            }
            if (categoryResponse.Data == null)
            {
                if (categoryResponse.Error == null)
                {
                    throw new Exception("Internal Server Error. Please try again later.");
                }
                throw new Exception(categoryResponse.Error.Error);

            }
            return categoryResponse.Data.Category;
        }

        public async Task<List<AuctionChatMessage>> GetHistoryForAuctionChat(Guid auctionId)
        {
            return await _chatMessageRepository.GetMessagesForAuctionChat(auctionId);
        }
        public async Task<string> CreateBid(Guid auctionId,Guid userId,int bidValue)
        {
            var bidCreateDto = new BidCreateDto
            { BidValue = bidValue };
            var url = $"api-auctions/auction/{auctionId}/create/bid/user/{userId}";
            var response = await _httpClient.PostAsJsonAsync(url, bidCreateDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiCreateBidResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<object>>(jsonString);
            if (!response.IsSuccessStatusCode)
            {
                if (apiCreateBidResponse?.Error != null) {
                    return (apiCreateBidResponse.Error.Details);
                }
                else { return ("Internal Server Error. Please try again later."); 
                }
            }
            else { return ("Success"); }

        }
        public async Task<string> DeleteBid(Guid auctionId, Guid userId)
        {
            var url = $"api-auctions/auction/{auctionId}/delete/bid/user/{userId}";
            var response = await _httpClient.DeleteAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();
            var apiDeleteBidResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<object>>(jsonString);

            if (!response.IsSuccessStatusCode)
            {
                if (apiDeleteBidResponse?.Error != null)
                {
                    return apiDeleteBidResponse.Error.Details;
                }
                else
                {
                    return "Internal Server Error. Please try again later.";
                }
            }
            else
            {
                return "Success";
            }
        }



        public async Task<bool> IsAuctionActive(Guid auctionId)
        {
            var response = await _httpClient.GetAsync($"api-auctions/isActive/auction/{auctionId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Internal Server Error. Please try again later.");
            }
            var isAuctionActive = await response.Content.ReadFromJsonAsync<bool>();

            return isAuctionActive;
        }
        public async Task SaveMessage(AuctionChatMessage auctionChatMessage)
        {
            await _chatMessageRepository.SaveMessage(auctionChatMessage);
        }
    }
}
