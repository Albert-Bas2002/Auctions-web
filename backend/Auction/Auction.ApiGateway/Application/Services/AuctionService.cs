using System.Net.Http.Json;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos;
using Auction.ApiGateway.Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auction.ApiGateway.Application.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUploadPath;

        public AuctionService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("AuctionService");
            var currentDir = Directory.GetCurrentDirectory();
            _baseUploadPath = Path.Combine(currentDir, "Photos");
        }
        public async Task<Result<AuctionPageDto>> GetAuctionPage(Guid auctionId, Guid userId)
        {

            var userCategory = await GetUserCategory(auctionId, userId);
            if (userCategory.IsFailure)
            {
                return Result.Failure<AuctionPageDto>(userCategory.Error);
            }
            else
            {
                switch (userCategory.Value)
                {
                    case "Guest":
                        return await HandleGuestCategory(auctionId);

                    case "Winner":
                    case "Creator":
                        return await HandleWinnerOrCreatorCategory(auctionId, userCategory.Value);

                    case "Bidder":
                        return await HandleBidderCategory(userId, auctionId);

                    default:
                        return Result.Failure<AuctionPageDto>("Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500");
                }
            }
        }

        private async Task<Result<AuctionPageDto>> HandleGuestCategory(Guid auctionId)
        {
            var response = await _httpClient.GetAsync($"api-auctions/auction/{auctionId}");
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiPageBaseResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<AuctionPageBaseDto>>(jsonString);
            var error = apiPageBaseResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || apiPageBaseResponse?.Data == null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<AuctionPageDto>(errorMessage);
            }
            var dto = apiPageBaseResponse.Data;
            return Result.Success(new AuctionPageDto
            {
                AuctionId = dto.AuctionId,
                CreationTime = dto.CreationTime,
                EndTime = dto.EndTime,
                Title = dto.Title,
                Description = dto.Description,
                Reserve = dto.Reserve,
                CurrentBid = dto.CurrentBid,
                Type = "Guest",
                BiddersBid = null,
                Status = "Active"
            });
        }

        private async Task<Result<AuctionPageDto>> HandleWinnerOrCreatorCategory(Guid auctionId, string role)
        {
            var url = $"api-auctions/auction/creator-winner/auction/{auctionId}";
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiPageStatusResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<AuctionPageStatusDto>>(jsonString);
            var error = apiPageStatusResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || apiPageStatusResponse?.Data == null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<AuctionPageDto>(errorMessage);
            }
            var dto = apiPageStatusResponse.Data;
            return Result.Success(new AuctionPageDto
            {
                AuctionId = dto.AuctionId,
                CreationTime = dto.CreationTime,
                EndTime = dto.EndTime,
                Title = dto.Title,
                Description = dto.Description,
                Reserve = dto.Reserve,
                CurrentBid = dto.CurrentBid,
                Type = role,
                BiddersBid = role == "Winner" ? dto.CurrentBid : null,
                Status = dto.Status
            });
        }

        private async Task<Result<AuctionPageDto>> HandleBidderCategory(Guid userId, Guid auctionId)
        {
            var url = $"api-auctions/auction/bidder/{userId}/auction/{auctionId}";
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiPageBidderResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<AuctionPageBidderDto>>(jsonString);
            var error = apiPageBidderResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || apiPageBidderResponse?.Data == null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<AuctionPageDto>(errorMessage);
            }

            var dto = apiPageBidderResponse.Data;
            return Result.Success(new AuctionPageDto
            {
                AuctionId = dto.AuctionId,
                CreationTime = dto.CreationTime,
                EndTime = dto.EndTime,
                Title = dto.Title,
                Description = dto.Description,
                Reserve = dto.Reserve,
                CurrentBid = dto.CurrentBid,
                Type = "Bidder",
                BiddersBid = dto.BiddersBid,
                Status = "Active"
            });
        }
        private async Task<Result<string>> GetUserCategory(Guid auctionId, Guid userId)
        {
            var url = $"api-auctions/get-category/auction/{auctionId}/user/{userId}";
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
            var categoryResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<UserCategoryForAuctionDto>>(jsonString);
            var error = categoryResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || categoryResponse?.Data == null)
            {
                var errorMessage = error != null
                 ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                 : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<string>(errorMessage);
            }
            else
            {
                var userCategory = categoryResponse.Data.Category;
                return Result.Success(userCategory);
            }
        }
        public async Task<Result<ContactDto>> GetCreatorOrWinnerId(Guid auctionId, Guid userId)
        {
            var url = $"api-auctions/auction/{auctionId}/user/{userId}/creator-winner-info";
            var response = await _httpClient.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
            var creatorOrWinnerIdResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<ContactDto>>(jsonString);
            var error = creatorOrWinnerIdResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || creatorOrWinnerIdResponse?.Data == null)
            {
                var errorMessage = error != null
                 ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                 : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<ContactDto>(errorMessage);
            }
            else
            {
                var creatorOrWinnerId = creatorOrWinnerIdResponse.Data;

                return Result.Success(creatorOrWinnerId);
            }
        }
        public async Task<Result> CreateAuction(Guid userId, AuctionCreateDto auctionCreateDto)
        {
            var url = $"api-auctions/auction/createBy/user/{userId}";
            var response = await _httpClient.PostAsJsonAsync(url, auctionCreateDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiCreateAuctionResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<object>>(jsonString);
            var error = apiCreateAuctionResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure(errorMessage);
            }
            return Result.Success();
        }
    

        public async Task<Result> AuctionCompleteDeal(Guid auctionId, Guid userId)
        {
            var url = $"api-auctions/auction/deal-complete/{auctionId}/user/{userId}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiAuctionCompleteDealResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<object>>(jsonString);
            var error = apiAuctionCompleteDealResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure(errorMessage);
            }
            return Result.Success();
        }
        public async Task<Result<List<AuctionListItemDto>>> GetAuctionsForUser(Guid userId,string category,bool? active = null)
        {
            var url = $"api-auctions/auctions/user/{userId}?category={category}&active={active}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiAuctionListItemResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<List<AuctionListItemDto>>>(jsonString);
            var error = apiAuctionListItemResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || apiAuctionListItemResponse?.Data == null)
            {
                var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<List<AuctionListItemDto>>(errorMessage);
            }
            return Result.Success(apiAuctionListItemResponse.Data);
        }
        public async Task<Result<List<AuctionListItemDto>>> GetAuctions(string? sortType, int page = 1)
        {
            var url = $"api-auctions/auctions?sortType={sortType}&page={page}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiAuctionListItemResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<List<AuctionListItemDto>>>(jsonString);
            var error = apiAuctionListItemResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null || apiAuctionListItemResponse?.Data == null)
            {
                var errorMessage = error != null
                    ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                    : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure<List<AuctionListItemDto>>(errorMessage);
            }
            return Result.Success(apiAuctionListItemResponse.Data);
        }

        public async Task<Result> CloseAuction(Guid auctionId, string userType, Guid? userId = null)
        {
            if (userType == "Creator")
            {
                if (!userId.HasValue)
                    return Result.Failure("Error: Missing Error, Details:  Missing User ID for Creator., Status: 400");

                var userCategory = await GetUserCategory(auctionId, userId.Value);
                if (userCategory.IsFailure)
                    return Result.Failure(userCategory.Error);

                if (userCategory.Value != "Creator")
                    return Result.Failure("Error: Close Error, Details: User can not close Auction, Status: 400");
            }

            var url = $"api-auctions/auction/close/{auctionId}?userType={userType}";
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var apiAuctionCloseResponse = JsonConvert.DeserializeObject<AuctionServiceApiResponse<object>>(jsonString);
            var error = apiAuctionCloseResponse?.Error;

            if (!response.IsSuccessStatusCode || error != null)
            {
                var errorMessage = error != null
                    ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                    : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";
                return Result.Failure(errorMessage);
            }

            return Result.Success();
        }
        public async Task<Result> IsAuctionActive(Guid auctionId)
        {
            var url = $"api-auctions/isActive/auction/{auctionId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure($"Error: Server returned {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            bool isActive = JsonConvert.DeserializeObject<bool>(jsonString);

            return isActive
                ? Result.Success()
                : Result.Failure("Auction is not active.");
        }


        public async Task<Result> UploadPhotosForAuction(Guid auctionId, List<IFormFile> photos, Guid userId)
        {
            if (photos == null || photos.Count == 0)
                return Result.Failure("No files.");

            if (photos.Count > 5)
                return Result.Failure("Too many files to upload. Maximum is 5.");

            var isAuctionActiveResult = await IsAuctionActive(auctionId);
            if (isAuctionActiveResult.IsFailure)
            {
                return Result.Failure(isAuctionActiveResult.Error);
            }
            var userCategoryResult = await GetUserCategory(auctionId, userId);
            if (userCategoryResult.IsFailure)
            {
                return Result.Failure(userCategoryResult.Error);
            }
            else
            {
                var userCategory = userCategoryResult.Value;
                if (userCategory == "Creator")
                {
                    var auctionFolder = Path.Combine(_baseUploadPath, auctionId.ToString());

                    if (!Directory.Exists(auctionFolder))
                        Directory.CreateDirectory(auctionFolder);

                    var existingFilesCount = Directory.GetFiles(auctionFolder).Length;

                    if (existingFilesCount + photos.Count > 5)
                        return Result.Failure("Too many photos for auction");

                    var savedFiles = new List<string>();

                    for (int i = 0; i < photos.Count; i++)
                    {
                        var photo = photos[i];

                        if (photo.Length == 0)
                            continue;

                        var extension = Path.GetExtension(photo.FileName);
                        var fileName = $"photo{existingFilesCount + i}{extension}";
                        var filePath = Path.Combine(auctionFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        savedFiles.Add(Path.Combine("Uploads", auctionId.ToString(), fileName));
                    }

                    return Result.Success();
                }
                else
                {
                    return Result.Failure("Only Creator can upload photos");
                }
            }
        }
        public async Task<Result> DeletePhotosForAuction(Guid auctionId, int[] indexesToDelete, Guid userId)
        {
            var isAuctionActiveResult = await IsAuctionActive(auctionId);
            if (isAuctionActiveResult.IsFailure)
                return Result.Failure(isAuctionActiveResult.Error);

            var userCategoryResult = await GetUserCategory(auctionId, userId);
            if (userCategoryResult.IsFailure)
                return Result.Failure(userCategoryResult.Error);

            if (userCategoryResult.Value != "Creator")
                return Result.Failure("Only Creator can delete photos.");

            var auctionFolder = Path.Combine(_baseUploadPath, auctionId.ToString());

            if (!Directory.Exists(auctionFolder))
                return Result.Failure("Auction folder not found.");

            var files = Directory.GetFiles(auctionFolder)
                                 .OrderBy(f => f)
                                 .ToList();

            if (indexesToDelete.Any(i => i < 0 || i >= files.Count))
                return Result.Failure("One or more indexes are out of range.");

            foreach (var index in indexesToDelete.Distinct().OrderByDescending(i => i))
            {
                try
                {
                    System.IO.File.Delete(files[index]);
                    files.RemoveAt(index);
                }
                catch (Exception ex)
                {
                    return Result.Failure($"Failed to delete file at index {index}: {ex.Message}");
                }
            }

            for (int i = 0; i < files.Count; i++)
            {
                var oldPath = files[i];
                var extension = Path.GetExtension(oldPath);
                var newPath = Path.Combine(auctionFolder, $"photo{i}{extension}");

                if (oldPath != newPath)
                {
                    try
                    {
                        System.IO.File.Move(oldPath, newPath);
                        files[i] = newPath;
                    }
                    catch (Exception ex)
                    {
                        return Result.Failure($"Failed to rename file {Path.GetFileName(oldPath)} to {Path.GetFileName(newPath)}: {ex.Message}");
                    }
                }
            }

            return Result.Success();
        }


    }
}

    


