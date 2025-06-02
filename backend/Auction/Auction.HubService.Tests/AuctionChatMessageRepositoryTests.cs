using Auction.HubService.Core.Models;
using Auction.HubService.Data;
using Auction.HubService.Data.Repositories;
using Microsoft.EntityFrameworkCore;

public class AuctionChatMessageRepositoryTests
{
    private DbContextOptions<AuctionChatDbContext> _dbContextOptions;

    public AuctionChatMessageRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AuctionChatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task SaveMessage_AddsMessageToDatabase()
    {
        using var context = new AuctionChatDbContext(_dbContextOptions);
        var repository = new AuctionChatMessageRepository(context);

        var message = AuctionChatMessage.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Buyer",
            "Test message");

        await repository.SaveMessage(message);

        var savedMessage = await context.AuctionChatMessages.FirstOrDefaultAsync();
        Assert.NotNull(savedMessage);
        Assert.Equal(message.MessageId, savedMessage.MessageId);
        Assert.Equal(message.Message, savedMessage.Message);
    }

    [Fact]
    public async Task GetMessagesForAuctionChat_ReturnsMessagesOrderedByTimestamp()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionChatDbContext(_dbContextOptions);
        var repository = new AuctionChatMessageRepository(context);

        var messages = new[]
        {
            AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-10), "Buyer", "First"),
            AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5), "Buyer", "Second"),
            AuctionChatMessage.Create(Guid.NewGuid(), auctionId, Guid.NewGuid(), DateTime.UtcNow, "Buyer", "Third")
        };

        foreach (var msg in messages)
            await repository.SaveMessage(msg);

        var result = await repository.GetMessagesForAuctionChat(auctionId);

        Assert.Equal(3, result.Count);
        Assert.Equal("First", result[0].Message);
        Assert.Equal("Second", result[1].Message);
        Assert.Equal("Third", result[2].Message);
    }

    [Fact]
    public async Task GetMessagesForAuctionChat_ReturnsEmptyList_WhenNoMessages()
    {
        using var context = new AuctionChatDbContext(_dbContextOptions);
        var repository = new AuctionChatMessageRepository(context);

        var messages = await repository.GetMessagesForAuctionChat(Guid.NewGuid());

        Assert.Empty(messages);
    }


    [Fact]
    public async Task GetMessagesForAuctionChat_LimitsTo50Messages()
    {
        var auctionId = Guid.NewGuid();
        using var context = new AuctionChatDbContext(_dbContextOptions);
        var repository = new AuctionChatMessageRepository(context);

        for (int i = 0; i < 60; i++)
        {
            var msg = AuctionChatMessage.Create(
                Guid.NewGuid(), auctionId, Guid.NewGuid(),
                DateTime.UtcNow.AddMinutes(-60 + i),
                "Buyer", $"Message {i}");
            await repository.SaveMessage(msg);
        }

        var messages = await repository.GetMessagesForAuctionChat(auctionId);

        Assert.Equal(50, messages.Count);
        Assert.Equal("Message 10", messages.First().Message);
        Assert.Equal("Message 59", messages.Last().Message);
    }
}
