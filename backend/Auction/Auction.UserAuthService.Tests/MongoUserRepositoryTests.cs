using System;
using System.Threading.Tasks;
using Xunit;
using Mongo2Go;
using MongoDB.Driver;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data.Repositories;

public class MongoUserRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoDatabase _database;
    private readonly MongoUserRepository _repository;

    public MongoUserRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDb");
        _repository = new MongoUserRepository(_database);
    }

    public void Dispose()
    {
        _runner.Dispose();
    }

    [Fact]
    public async Task Add_And_GetByEmail_Works()
    {
        var user = User.Create(Guid.NewGuid(), "test", "hash", "test@example.com", "123");
        await _repository.Add(user);

        var exists = await _repository.UserExistsByEmail("test@example.com");
        Assert.True(exists);

        var retrieved = await _repository.GetByEmail("test@example.com");
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
    }

    [Fact]
    public async Task GetUsersPermissions_ReturnsExpectedPermissions()
    {
        var user = User.Create(Guid.NewGuid(), "perm", "hash", "perm@example.com", "contact");
        await _repository.Add(user);

        var perms = await _repository.GetUsersPermissions(user.UserId);
        Assert.Contains("User-Permission", perms);
        Assert.Single(perms);
    }
    [Fact]
    public async Task Add_Throws_When_EmailAlreadyExists()
    {
        var user1 = User.Create(Guid.NewGuid(), "user1", "hash", "duplicate@example.com", "123");
        var user2 = User.Create(Guid.NewGuid(), "user2", "hash", "duplicate@example.com", "456");

        await _repository.Add(user1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.Add(user2));
        Assert.Equal("A user with this email already exists.", ex.Message);
    }
    [Fact]
    public async Task GetByUserId_Returns_CorrectUser()
    {
        var user = User.Create(Guid.NewGuid(), "byId", "hash", "byid@example.com", "contact");
        await _repository.Add(user);

        var result = await _repository.GetByUserId(user.UserId);

        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal(user.Email, result.Email);
    }
    [Fact]
    public async Task GetByUserId_Returns_Null_IfNotFound()
    {
        var result = await _repository.GetByUserId(Guid.NewGuid());
        Assert.Null(result);
    }
    [Fact]
    public async Task Update_Updates_ExistingUser()
    {
        var user = User.Create(Guid.NewGuid(), "old", "hash", "old@example.com", "oldContact");
        await _repository.Add(user);

        var updated = User.Create(user.UserId, "newName", "newHash", "new@example.com", "newContact");
        await _repository.Update(updated);

        var result = await _repository.GetByUserId(user.UserId);

        Assert.Equal("newName", result.UserName);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal("newHash", result.PasswordHash);
        Assert.Equal("newContact", result.Contacts);
    }
    [Fact]
    public async Task UserExistsByEmail_Throws_On_EmptyEmail()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.UserExistsByEmail(""));
    }
    [Fact]
    public async Task GetByEmail_Returns_Null_IfNotFound()
    {
        var result = await _repository.GetByEmail("notfound@example.com");
        Assert.Null(result);
    }
    [Fact]
    public async Task Add_Throws_If_User_Is_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.Add(null!));
    }

}
