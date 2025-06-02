using Auction.HubService.Application.Services;
using Auction.HubService.Contracts;
using Auction.HubService.Core.Abstractions;
using Auction.HubService.Core.Models;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class AuctionHubServiceTests
{
    private readonly Mock<IAuctionChatMessageRepository> _messageRepoMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public AuctionHubServiceTests()
    {
        _messageRepoMock = new Mock<IAuctionChatMessageRepository>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://fakeapi.com/")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient("AuctionService")).Returns(_httpClient);
    }

    [Fact]
    public async Task GetUserCategoryForAuction_ReturnsCategory_WhenSuccessful()
    {
        var response = new AuctionServiceApiResponse<UserCategoryForAuctionDto>
        {
            Data = new UserCategoryForAuctionDto { Category = "Bidder" }
        };
        var json = JsonConvert.SerializeObject(response);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);
        var result = await service.GetUserCategoryForAuction(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("Bidder", result);
    }

    [Fact]
    public async Task GetUserCategoryForAuction_Throws_WhenResponseFails()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);

        await Assert.ThrowsAsync<Exception>(() =>
            service.GetUserCategoryForAuction(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateBid_ReturnsSuccess_WhenResponseOk()
    {
        var json = JsonConvert.SerializeObject(new AuctionServiceApiResponse<object>());
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);
        var result = await service.CreateBid(Guid.NewGuid(), Guid.NewGuid(), 100);

        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task CreateBid_ReturnsErrorDetails_WhenErrorReturned()
    {
        var json = JsonConvert.SerializeObject(new AuctionServiceApiResponse<object>
        {
            Error = new ErrorDto { Details = "Bid too low" }
        });
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);
        var result = await service.CreateBid(Guid.NewGuid(), Guid.NewGuid(), 5);

        Assert.Equal("Bid too low", result);
    }

    [Fact]
    public async Task IsAuctionActive_ReturnsTrue_WhenResponseIsTrue()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(true)
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);
        var result = await service.IsAuctionActive(Guid.NewGuid());

        Assert.True(result);
    }

    [Fact]
    public async Task SaveMessage_CallsRepository()
    {
        var message = AuctionChatMessage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "Bidder", "Test message");
        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);

        await service.SaveMessage(message);

        _messageRepoMock.Verify(r => r.SaveMessage(message), Times.Once);
    }
    [Fact]
    public async Task DeleteBid_ReturnsSuccess_WhenResponseIsSuccessful()
    {
        var response = new AuctionServiceApiResponse<object>(); 
        var json = JsonConvert.SerializeObject(response);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);

        var result = await service.DeleteBid(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("Success", result);
    }
    [Fact]
    public async Task DeleteBid_ReturnsErrorDetails_WhenApiReturnsError()
    {
        var response = new AuctionServiceApiResponse<object>
        {
            Error = new ErrorDto { Details = "Bid not found", Status = 404 }
        };
        var json = JsonConvert.SerializeObject(response);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);

        var result = await service.DeleteBid(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("Bid not found", result);
    }
    [Fact]
    public async Task GetHistoryForAuctionChat_ReturnsMessages()
    {
        var auctionId = Guid.NewGuid();
        var fakeMessages = new List<AuctionChatMessage>
    {
        AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow, "Buyer", "Hello"),
        AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow, "Seller", "Hi")
    };

        _messageRepoMock.Setup(repo => repo.GetMessagesForAuctionChat(auctionId))
            .ReturnsAsync(fakeMessages);

        var service = new AuctionHubService(_httpClientFactoryMock.Object, _messageRepoMock.Object);

        var result = await service.GetHistoryForAuctionChat(auctionId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, msg => msg.Message == "Hello");
        Assert.Contains(result, msg => msg.Message == "Hi");
    }



}
