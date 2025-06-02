using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Auction.ApiGateway.Application.Services;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos;
using CSharpFunctionalExtensions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

public class AuctionServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly AuctionService _auctionService;

    public AuctionServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://fake-api/")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);

        _auctionService = new AuctionService(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetAuctionPage_Should_Return_GuestDto_When_Category_Is_Guest()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var userCategoryResponse = new AuctionServiceApiResponse<UserCategoryForAuctionDto>
        {
            Data = new UserCategoryForAuctionDto { Category = "Guest" }
        };

        var auctionPageResponse = new AuctionServiceApiResponse<AuctionPageBaseDto>
        {
            Data = new AuctionPageBaseDto
            {
                AuctionId = auctionId,
                CreationTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                Title = "Test Auction",
                Description = "Test Description",
                Reserve = 100,
                CurrentBid = 150
            }
        };

        SetupHttpResponse($"api-auctions/get-category/auction/{auctionId}/user/{userId}", userCategoryResponse);
        SetupHttpResponse($"api-auctions/auction/{auctionId}", auctionPageResponse);

        // Act
        var result = await _auctionService.GetAuctionPage(auctionId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Guest", result.Value.Type);
        Assert.Equal(auctionId, result.Value.AuctionId);
        Assert.Equal("Test Auction", result.Value.Title);

    }

    [Fact]
    public async Task GetAuctionPage_Should_Return_Error_When_GetUserCategory_Fails()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var errorResponse = new AuctionServiceApiResponse<UserCategoryForAuctionDto>
        {
            Error = new ErrorDto
            {
                Error = "Unauthorized",
                Details = "Access denied",
                Status = 401
            }
        };

        SetupHttpResponse($"api-auctions/get-category/auction/{auctionId}/user/{userId}", errorResponse, HttpStatusCode.Unauthorized);

        // Act
        var result = await _auctionService.GetAuctionPage(auctionId, userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Unauthorized", result.Error);

    }

    [Fact]
    public async Task GetCreatorOrWinnerId_Should_Return_ContactDto_When_Successful()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var contactDto = new ContactDto { ContactId = Guid.NewGuid() };
        var response = new AuctionServiceApiResponse<ContactDto> { Data = contactDto };

        SetupHttpResponse($"api-auctions/auction/{auctionId}/user/{userId}/creator-winner-info", response);

        // Act
        var result = await _auctionService.GetCreatorOrWinnerId(auctionId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(contactDto.ContactId, result.Value.ContactId);

    }
    [Fact]
    public async Task HandleBidderCategory_Should_Return_BidderDto_When_Successful()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var userCategoryResponse = new AuctionServiceApiResponse<UserCategoryForAuctionDto>
        {
            Data = new UserCategoryForAuctionDto { Category = "Bidder" }
        };

        var auctionBidderResponse = new AuctionServiceApiResponse<AuctionPageBidderDto>
        {
            Data = new AuctionPageBidderDto
            {
                AuctionId = auctionId,
                CreationTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(1),
                Title = "Bidder Auction",
                Description = "Description",
                Reserve = 200,
                CurrentBid = 300,
                BiddersBid = 250
            }
        };

        SetupHttpResponse($"api-auctions/get-category/auction/{auctionId}/user/{userId}", userCategoryResponse);
        SetupHttpResponse($"api-auctions/auction/bidder/{userId}/auction/{auctionId}", auctionBidderResponse);

        // Act
        var result = await _auctionService.GetAuctionPage(auctionId, userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Bidder", result.Value.Type);
        Assert.Equal(250, result.Value.BiddersBid);
    }

    [Fact]
    public async Task CreateAuction_Should_Return_Success_When_Api_Response_Is_Success()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auctionCreateDto = new AuctionCreateDto
        {
            Title = "New Auction",
            Description = "New Auction Description",
            Reserve = 100
        };

        var apiResponse = new AuctionServiceApiResponse<object> { Data = new object() };

        SetupHttpResponse($"api-auctions/auction/createBy/user/{userId}", apiResponse);

        // Act
        var result = await _auctionService.CreateAuction(userId, auctionCreateDto);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateAuction_Should_Return_Failure_When_Api_Response_Has_Error()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var auctionCreateDto = new AuctionCreateDto
        {
            Title = "New Auction",
            Description = "New Auction Description",
            Reserve = 100
        };

        var errorResponse = new AuctionServiceApiResponse<object>
        {
            Error = new ErrorDto { Error = "Bad Request", Details = "Invalid Data", Status = 400 }
        };

        SetupHttpResponse($"api-auctions/auction/createBy/user/{userId}", errorResponse, HttpStatusCode.BadRequest);

        // Act
        var result = await _auctionService.CreateAuction(userId, auctionCreateDto);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Bad Request", result.Error);
    }

    [Fact]
    public async Task IsAuctionActive_Should_Return_Success_When_Active()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var responseContent = JsonConvert.SerializeObject(true);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"api-auctions/isActive/auction/{auctionId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _auctionService.IsAuctionActive(auctionId);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsAuctionActive_Should_Return_Failure_When_NotActive()
    {
        // Arrange
        var auctionId = Guid.NewGuid();
        var responseContent = JsonConvert.SerializeObject(false);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"api-auctions/isActive/auction/{auctionId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _auctionService.IsAuctionActive(auctionId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Auction is not active.", result.Error);
    }
    [Fact]
    public async Task CloseAuction_Should_Return_Success_When_Api_Response_Is_Success()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var apiResponse = new AuctionServiceApiResponse<object> { Data = new object() };

        SetupHttpResponse($"api-auctions/auction/close/{auctionId}", apiResponse);

        var result = await _auctionService.CloseAuction(auctionId, userId);

        
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CloseAuction_Should_Return_Failure_When_Api_Response_Has_Error()
    {
        var auctionId = Guid.NewGuid();
        var userType = "Creator";

        var errorResponse = new AuctionServiceApiResponse<object>
        {
            Error = new ErrorDto { Error = "Forbidden", Details = "Cannot close auction", Status = 403 }
        };

        SetupHttpResponse($"api-auctions/auction/close/{auctionId}?userType={userType}", errorResponse, HttpStatusCode.Forbidden);

        var result = await _auctionService.CloseAuction(auctionId, userType);

        Assert.True(result.IsFailure);

    }


    [Fact]
    public async Task AuctionCompleteDeal_Should_Return_Success_When_Api_Response_Is_Success()
    {
        
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var apiResponse = new AuctionServiceApiResponse<object> { Data = new object() };

        SetupHttpResponse($"api-auctions/auction/deal-complete/{auctionId}/user/{userId}", apiResponse);

        var result = await _auctionService.AuctionCompleteDeal(auctionId, userId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AuctionCompleteDeal_Should_Return_Failure_When_Api_Response_Has_Error()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var errorResponse = new AuctionServiceApiResponse<object>
        {
            Error = new ErrorDto { Error = "Conflict", Details = "Deal cannot be completed", Status = 409 }
        };

        SetupHttpResponse($"api-auctions/auction/deal-complete/{auctionId}/user/{userId}", errorResponse, HttpStatusCode.Conflict);

        var result = await _auctionService.AuctionCompleteDeal(auctionId, userId);

        
        Assert.True(result.IsFailure);
        Assert.Contains("Conflict", result.Error);
    }


    private void SetupHttpResponse<T>(string expectedUrl, T content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(expectedUrl)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
            })
            .Verifiable();
    }
}
