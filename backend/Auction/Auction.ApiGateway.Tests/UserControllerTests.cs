using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Auction.ApiGateway.Controllers;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using Auction.ApiGateway.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts;
using CSharpFunctionalExtensions;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IErrorMessageParser> _errorMessageParserMock = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_userServiceMock.Object, _errorMessageParserMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", Guid.NewGuid().ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task UserInfo_HidesEmail_WhenUserIdDoesNotMatchClaim()
    {
        var routeUserId = Guid.NewGuid();
        var claimUserId = Guid.NewGuid().ToString();

        var userInfo = new UserInfoDto
        {
            Email = "real@email.com",
            UserName = "username"
        };

        _userServiceMock.Setup(x => x.GetUserInfo(routeUserId))
            .ReturnsAsync(Result.Success(userInfo));

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", claimUserId)
        }, "mock"));

        var result = await _controller.UserInfo(routeUserId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<UserInfoDto>>(okResult.Value);

        Assert.Equal("Hidden@hidden.com", response.Data.Email);
    }

    [Fact]
    public async Task UserInfo_ShowsRealEmail_WhenUserIdMatchesClaim()
    {
        var userId = Guid.NewGuid();

        var userInfo = new UserInfoDto
        {
            Email = "real@email.com",
            UserName = "username"
        };

        _userServiceMock.Setup(x => x.GetUserInfo(userId))
            .ReturnsAsync(Result.Success(userInfo));

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", userId.ToString())
        }, "mock"));

        var result = await _controller.UserInfo(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<UserInfoDto>>(okResult.Value);

        Assert.Equal("real@email.com", response.Data.Email);
    }

    [Fact]
    public async Task UserInfo_ReturnsError_WhenServiceFails()
    {
        var userId = Guid.NewGuid();
        var errorMsg = "error happened";
        var errorDto = new ErrorDto { Error = "Err", Details = "Details", Status = 400 };

        _userServiceMock.Setup(x => x.GetUserInfo(userId))
            .ReturnsAsync(Result.Failure<UserInfoDto>(errorMsg));

        _errorMessageParserMock.Setup(p => p.ParseMessageToErrorDto(errorMsg))
            .Returns(errorDto);

        var result = await _controller.UserInfo(userId);

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objResult.StatusCode);

        var response = Assert.IsType<ApiGatewayResponse<UserInfoDto>>(objResult.Value);
        Assert.Equal(errorDto, response.Error);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenSuccess()
    {
        var request = new RegisterUserRequest
        {
            UserName = "test",
            Email = "test@mail.com",
            Password = "pass",
            Contacts = "contacts"
        };

        _userServiceMock.Setup(x => x.Register(It.IsAny<RegisterUserDto>()))
            .ReturnsAsync(Result.Success<object>(null));

        var result = await _controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }

    [Fact]
    public async Task Register_ReturnsError_WhenFails()
    {
        var request = new RegisterUserRequest
        {
            UserName = "test",
            Email = "test@mail.com",
            Password = "pass",
            Contacts = "contacts"
        };

        var errorMsg = "fail";
        var errorDto = new ErrorDto { Error = "Err", Details = "Details", Status = 409 };

        _userServiceMock.Setup(x => x.Register(It.IsAny<RegisterUserDto>()))
            .ReturnsAsync(Result.Failure<object>(errorMsg));

        _errorMessageParserMock.Setup(p => p.ParseMessageToErrorDto(errorMsg))
            .Returns(errorDto);

        var result = await _controller.Register(request);

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(409, objResult.StatusCode);

        var response = Assert.IsType<ApiGatewayResponse<object>>(objResult.Value);
        Assert.Equal(errorDto, response.Error);
    }

    [Fact]
    public async Task Login_ReturnsError_WhenFails()
    {
        var request = new LoginUserRequest
        {
            Email = "test@mail.com",
            Password = "pass"
        };

        var errorMsg = "fail login";
        var errorDto = new ErrorDto { Error = "Err", Details = "Details", Status = 401 };
        
        _userServiceMock.Setup(x => x.Login(It.IsAny<LoginUserDto>()))
            .ReturnsAsync(Result.Failure<LoginTokenDto>(errorMsg));

        _errorMessageParserMock.Setup(p => p.ParseMessageToErrorDto(errorMsg))
            .Returns(errorDto);

        var result = await _controller.Login(request);

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(401, objResult.StatusCode);

        var response = Assert.IsType<ApiGatewayResponse<object>>(objResult.Value);
        Assert.Equal(errorDto, response.Error);
    }

    [Fact]
    public async Task ChangeUserName_ReturnsUnauthorized_WhenUserIdMissing()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var request = new ChangeUserNameRequest { NewUserName = "newname" };

        var result = await _controller.ChangeUserName(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(unauthorizedResult.Value);

        Assert.Equal(401, response.Error.Status);
        Assert.Equal("Invalid token", response.Error.Error);
    }

    [Fact]
    public async Task ChangeUserName_ReturnsOk_WhenSuccess()
    {
        var userId = Guid.NewGuid();

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", userId.ToString())
        }, "mock"));

        var request = new ChangeUserNameRequest { NewUserName = "newname" };

        _userServiceMock.Setup(x => x.ChangeUserName(It.IsAny<ChangeUserNameDto>()))
            .ReturnsAsync(Result.Success<object>(null));

        var result = await _controller.ChangeUserName(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }

    [Fact]
    public async Task ChangeEmail_ReturnsOk_WhenSuccess()
    {
        var userId = Guid.NewGuid();

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
            new Claim("userId", userId.ToString())
            }, "mock"));

        var request = new ChangeEmailRequest { NewEmail = "newemail@mail.com" };

        _userServiceMock.Setup(x => x.ChangeEmail(It.IsAny<ChangeEmailDto>()))
            .ReturnsAsync(Result.Success<object>(null));

        var result = await _controller.ChangeEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }

    [Fact]
    public async Task ChangeContacts_ReturnsOk_WhenSuccess()
    {
        var userId = Guid.NewGuid();

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
            new Claim("userId", userId.ToString())
            }, "mock"));

        var request = new ChangeContactsRequest { NewContacts = "123456789" };

        _userServiceMock.Setup(x => x.ChangeContacts(It.IsAny<ChangeContactsDto>()))
            .ReturnsAsync(Result.Success<object>(null));

        var result = await _controller.ChangeContacts(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }

    [Fact]
    public async Task ChangePassword_ReturnsOk_WhenSuccess()
    {
        var userId = Guid.NewGuid();

        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
            new Claim("userId", userId.ToString())
            }, "mock"));

        var request = new ChangePasswordRequest
        {
            PreviousPassword = "oldpass",
            NewPassword = "newpass"
        };

        _userServiceMock.Setup(x => x.ChangePassword(It.IsAny<ChangePasswordDto>()))
            .ReturnsAsync(Result.Success<object>(null));

        var result = await _controller.ChangePassword(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }

    [Fact]
    public void IsTokenValid_ReturnsTrue_WhenUserIsAuthenticated()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", Guid.NewGuid().ToString())
        }, "mock"));

        var result = _controller.IsTokenValid();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<bool>>(okResult.Value);

        Assert.True(response.Data);
    }

    [Fact]
    public void IsTokenValid_ReturnsFalse_WhenUserIsNotAuthenticated()
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = _controller.IsTokenValid();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<bool>>(okResult.Value);

        Assert.False(response.Data);
    }
}
