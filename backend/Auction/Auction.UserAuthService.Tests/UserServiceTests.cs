using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Auction.UserAuthService.Application.Services;
using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Contracts.Dtos;
using CSharpFunctionalExtensions;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsFailure_WhenEmailExists()
    {
        var email = "test@example.com";
        _userRepositoryMock.Setup(r => r.UserExistsByEmail(email)).ReturnsAsync(true);

        var result = await _userService.Register("user1", email, "password", "contacts");

        Assert.False(result.IsSuccess);
        Assert.Contains("A user with this email already exists.", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_ReturnsSuccess_WhenEmailDoesNotExist()
    {
        var email = "test@example.com";
        _userRepositoryMock.Setup(r => r.UserExistsByEmail(email)).ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.Generate(It.IsAny<string>())).Returns("hashedPassword");
        _userRepositoryMock.Setup(r => r.Add(It.IsAny<User>())).Returns(Task.CompletedTask);

        var result = await _userService.Register("user1", email, "password", "contacts");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ReturnsFailure_WhenUserNotFound()
    {
        var email = "test@example.com";
        _userRepositoryMock.Setup(r => r.GetByEmail(email)).ReturnsAsync((User)null);

        var result = await _userService.Login(email, "password");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ReturnsFailure_WhenPasswordIncorrect()
    {
        var email = "test@example.com";
        var user = User.Create(Guid.NewGuid(), "user1", "correctHash", email, "contacts");
        _userRepositoryMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await _userService.Login(email, "wrongPassword");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ReturnsSuccess_WhenCredentialsCorrect()
    {
        var email = "test@example.com";
        var user = User.Create(Guid.NewGuid(), "user1", "correctHash", email, "contacts");
        var permissions = new List<string> { "perm1", "perm2" };
        _userRepositoryMock.Setup(r => r.GetByEmail(email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _userRepositoryMock.Setup(r => r.GetUsersPermissions(user.UserId)).ReturnsAsync(permissions);
        _jwtProviderMock.Setup(j => j.GenerateToken(user, permissions)).Returns("jwtToken");

        var result = await _userService.Login(email, "correctPassword");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task ChangePassword_ReturnsFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.ChangePassword(userId, "newPass", "oldPass");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ChangePassword_ReturnsFailure_WhenPreviousPasswordIncorrect()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "user1", "hashOldPass", "email@mail.ru", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("oldPass", "hashOldPass")).Returns(false);

        var result = await _userService.ChangePassword(userId, "newPass", "oldPass");

        Assert.False(result.IsSuccess);
    }

    [Fact]

    public async Task ChangePassword_ReturnsFailure_WhenNewPasswordSameAsOldPassword()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "user1", "hashOldPass", "email@mail.ru", "contacts");

        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("oldPass", "hashOldPass")).Returns(true);

        _passwordHasherMock.Setup(h => h.Generate("oldPass")).Returns("hashOldPass");

        var result = await _userService.ChangePassword(userId, "oldPass", "oldPass");

        Assert.False(result.IsSuccess);
    }



    [Fact]
    public async Task ChangePassword_ReturnsSuccess_WhenPasswordChanged()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "user1", "hashOldPass", "email@mail.ru", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify("oldPass", "hashOldPass")).Returns(true);
        _passwordHasherMock.Setup(h => h.Generate("newPass")).Returns("hashNewPass");
        _passwordHasherMock.Setup(h => h.Generate("oldPass")).Returns("hashOldPass");
        _userRepositoryMock.Setup(r => r.Update(user)).Returns(Task.CompletedTask);

        var result = await _userService.ChangePassword(userId, "newPass", "oldPass");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeUserName_ReturnsFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.ChangeUserName(userId, "newName");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeUserName_ReturnsSuccess_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "oldName", "hash", "email@mail.ru", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.Update(user)).Returns(Task.CompletedTask);

        var result = await _userService.ChangeUserName(userId, "newName");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeContacts_ReturnsFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.ChangeContacts(userId, "newContacts");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeContacts_ReturnsSuccess_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "name", "hash", "email@mail.ru", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.Update(user)).Returns(Task.CompletedTask);

        var result = await _userService.ChangeContacts(userId, "newContacts");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeEmail_ReturnsFailure_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.ChangeEmail(userId, "newEmail@example.com");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeEmail_ReturnsSuccess_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "name", "hash", "oldEmail@example.com", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.Update(user)).Returns(Task.CompletedTask);

        var result = await _userService.ChangeEmail(userId, "newEmail@example.com");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task IsUserByUserId_ReturnsTrue_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "name", "hash", "email@mail.ru", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);

        var result = await _userService.IsUserByUserId(userId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsUserByUserId_ReturnsFalse_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.IsUserByUserId(userId);

        Assert.False(result);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsSuccess_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = User.Create(userId, "user1", "hash", "email@example.com", "contacts");
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync(user);

        var result = await _userService.GetUserInfo(userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Email, result.Value.Email);
        Assert.Equal(user.UserName, result.Value.UserName);
        Assert.Equal(user.Contacts, result.Value.Contacts);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsFailure_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(r => r.GetByUserId(userId)).ReturnsAsync((User)null);

        var result = await _userService.GetUserInfo(userId);

        Assert.False(result.IsSuccess);
    }
}
