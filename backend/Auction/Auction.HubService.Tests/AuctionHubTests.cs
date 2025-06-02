using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Auction.HubService.Application.Hubs;
using Auction.HubService.Core.Abstractions;
using Auction.HubService.Core.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class AuctionHubTests
{
    private readonly Mock<IAuctionHubService> _auctionHubServiceMock;
    private readonly Mock<ILogger<AuctionHub>> _loggerMock;
    private readonly Mock<IHubCallerClients<IClient>> _clientsMock;
    private readonly Mock<IClient> _callerClientMock;
    private readonly Mock<IGroupManager> _groupsMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly AuctionHub _hub;

    public AuctionHubTests()
    {
        _auctionHubServiceMock = new Mock<IAuctionHubService>();
        _loggerMock = new Mock<ILogger<AuctionHub>>();

        _clientsMock = new Mock<IHubCallerClients<IClient>>();
        _callerClientMock = new Mock<IClient>();
        _clientsMock.Setup(c => c.Caller).Returns(_callerClientMock.Object);

        _groupsMock = new Mock<IGroupManager>();

        _contextMock = new Mock<HubCallerContext>();

        _hub = new AuctionHub(_auctionHubServiceMock.Object, _loggerMock.Object)
        {
            Clients = _clientsMock.Object,
            Groups = _groupsMock.Object,
            Context = _contextMock.Object
        };
    }

    [Fact]
    public async Task JoinAuctionGroup_AddsUserToGroupAndSendsHistory_WhenAuctionIsActive()
    {
        var auctionId = Guid.NewGuid();
        var connectionId = "connection-1";
        _contextMock.Setup(c => c.ConnectionId).Returns(connectionId);

        var messages = new List<AuctionChatMessage>
        {
            AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow, "Buyer", "Hi")
        };

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(true);
        _auctionHubServiceMock.Setup(s => s.GetHistoryForAuctionChat(auctionId)).ReturnsAsync(messages);
        _groupsMock.Setup(g => g.AddToGroupAsync(connectionId, auctionId.ToString(), default)).Returns(Task.CompletedTask);
        _callerClientMock.Setup(c => c.ReceiveMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

        await _hub.JoinAuctionGroup(auctionId);

        _groupsMock.Verify(g => g.AddToGroupAsync(connectionId, auctionId.ToString(), default), Times.Once);
        _callerClientMock.Verify(c => c.ReceiveMessage("Buyer", "Hi", It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task JoinAuctionGroup_DoesNotAddToGroup_WhenAuctionIsNotActive()
    {
        var auctionId = Guid.NewGuid();
        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(false);

        await _hub.JoinAuctionGroup(auctionId);

        _groupsMock.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        _callerClientMock.Verify(c => c.ReceiveMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task SendMessageAuctionChat_SendsMessageAndSaves_WhenAuctionIsActive()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var message = "Hello";

        var claims = new[] { new Claim("userId", userId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _contextMock.Setup(c => c.User).Returns(principal);
        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(true);
        _auctionHubServiceMock.Setup(s => s.GetUserCategoryForAuction(userId, auctionId)).ReturnsAsync("Buyer");
        _clientsMock.Setup(c => c.Group(auctionId.ToString())).Returns(_callerClientMock.Object);
        _callerClientMock.Setup(c => c.ReceiveMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
        _auctionHubServiceMock.Setup(s => s.SaveMessage(It.IsAny<AuctionChatMessage>())).Returns(Task.CompletedTask);

        await _hub.SendMessageAuctionChat(auctionId, message);

        _clientsMock.Verify(c => c.Group(auctionId.ToString()), Times.Once);
        _callerClientMock.Verify(c => c.ReceiveMessage("Buyer", message, It.IsAny<DateTime>()), Times.Once);
        _auctionHubServiceMock.Verify(s => s.SaveMessage(It.Is<AuctionChatMessage>(m => m.Message == message && m.UserCategoryForAuction == "Buyer")), Times.Once);
    }

    [Fact]
    public async Task SendMessageAuctionChat_DoesNothing_WhenAuctionIsNotActive()
    {
        var auctionId = Guid.NewGuid();

        var claims = new[] { new Claim("userId", Guid.NewGuid().ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextMock.Setup(c => c.User).Returns(principal);

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(false);

        await _hub.SendMessageAuctionChat(auctionId, "Hello");

        _clientsMock.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
        _auctionHubServiceMock.Verify(s => s.SaveMessage(It.IsAny<AuctionChatMessage>()), Times.Never);
    }

    [Fact]
    public async Task CreateBid_ReturnsUnauthorized_WhenNoUserIdClaim()
    {
        _contextMock.Setup(c => c.User).Returns((ClaimsPrincipal)null);

        var result = await _hub.CreateBid(Guid.NewGuid(), 10);

        Assert.Equal("Unauthorized: No user ID claim found", result);
    }

    [Fact]
    public async Task CreateBid_ReturnsAuctionNotActive_WhenAuctionIsNotActive()
    {
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim("userId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextMock.Setup(c => c.User).Returns(principal);

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _hub.CreateBid(Guid.NewGuid(), 10);

        Assert.Equal("Auction is not active", result);
    }

    [Fact]
    public async Task CreateBid_ReturnsSuccess_WhenBidCreated()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim("userId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextMock.Setup(c => c.User).Returns(principal);

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(true);
        _auctionHubServiceMock.Setup(s => s.CreateBid(auctionId, userId, 10)).ReturnsAsync("Success");
        _clientsMock.Setup(c => c.Group(auctionId.ToString())).Returns(_callerClientMock.Object);
        _callerClientMock.Setup(c => c.ReceiveBidUpdate()).Returns(Task.CompletedTask);

        var result = await _hub.CreateBid(auctionId, 10);

        Assert.Equal("Bid sent successfully", result);
        _callerClientMock.Verify(c => c.ReceiveBidUpdate(), Times.Once);
    }

    [Fact]
    public async Task DeleteBid_ReturnsUnauthorized_WhenNoUserIdClaim()
    {
        _contextMock.Setup(c => c.User).Returns((ClaimsPrincipal)null);

        var result = await _hub.DeleteBid(Guid.NewGuid());

        Assert.Equal("Unauthorized: No user ID claim found", result);
    }

    [Fact]
    public async Task DeleteBid_ReturnsAuctionNotActive_WhenAuctionIsNotActive()
    {
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim("userId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextMock.Setup(c => c.User).Returns(principal);

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(It.IsAny<Guid>())).ReturnsAsync(false);

        var result = await _hub.DeleteBid(Guid.NewGuid());

        Assert.Equal("Auction is not active", result);
    }

    [Fact]
    public async Task DeleteBid_ReturnsSuccess_WhenBidDeleted()
    {
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim("userId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _contextMock.Setup(c => c.User).Returns(principal);

        _auctionHubServiceMock.Setup(s => s.IsAuctionActive(auctionId)).ReturnsAsync(true);
        _auctionHubServiceMock.Setup(s => s.DeleteBid(auctionId, userId)).ReturnsAsync("Success");
        _clientsMock.Setup(c => c.Group(auctionId.ToString())).Returns(_callerClientMock.Object);
        _callerClientMock.Setup(c => c.ReceiveBidUpdate()).Returns(Task.CompletedTask);

        var result = await _hub.DeleteBid(auctionId);

        Assert.Equal("Bid deleted successfully", result);
        _callerClientMock.Verify(c => c.ReceiveBidUpdate(), Times.Once);
    }

    [Fact]
    public async Task LeaveAuctionGroup_RemovesUserFromGroup()
    {
        var auctionId = Guid.NewGuid();
        var connectionId = "connection-1";

        _contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        _groupsMock.Setup(g => g.RemoveFromGroupAsync(connectionId, auctionId.ToString(), default)).Returns(Task.CompletedTask);

        await _hub.LeaveAuctionGroup(auctionId);

        _groupsMock.Verify(g => g.RemoveFromGroupAsync(connectionId, auctionId.ToString(), default), Times.Once);
    }
}
