using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Auction.ApiGateway.Application.Services;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using CSharpFunctionalExtensions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

public class UserServiceTests
{
    private readonly HttpClient _httpClient;
    private readonly UserService _userService;
    private readonly Mock<HttpMessageHandler> _handlerMock;

    public UserServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(_ => _.CreateClient("UserAuthService")).Returns(_httpClient);

        _userService = new UserService(httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        var registerDto = new RegisterUserDto
        {
            UserName = "test",
            Email = "test@example.com",
            Password = "pass123",
            Contacts = "1234567890"
        };

        var responseObj = new UserAuthApiResponse<object> { Data = new object() };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.Register(registerDto);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
    {
        var loginDto = new LoginUserDto { Email = "test@example.com", Password = "pass123" };
        var loginToken = new LoginTokenDto { Token = "fake-jwt-token" };
        var responseObj = new UserAuthApiResponse<LoginTokenDto> { Data = loginToken };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.Login(loginDto);

        Assert.True(result.IsSuccess);
        Assert.Equal("fake-jwt-token", result.Value.Token);
    }

    [Fact]
    public async Task GetUserInfo_ShouldReturnUserInfo_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var userInfo = new UserInfoDto { UserName = "test", Email = "test@example.com", Contacts = "123" };
        var responseObj = new UserAuthApiResponse<UserInfoDto> { Data = userInfo };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.GetUserInfo(userId);

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value.UserName);
    }

    [Fact]
    public async Task ChangeUserName_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        var dto = new ChangeUserNameDto { UserId = Guid.NewGuid(), NewUserName = "newName" };
        var responseObj = new UserAuthApiResponse<object> { Data = new object() };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.ChangeUserName(dto);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeContacts_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        var dto = new ChangeContactsDto { UserId = Guid.NewGuid(), NewContacts = "new contact" };
        var responseObj = new UserAuthApiResponse<object> { Data = new object() };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.ChangeContacts(dto);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeEmail_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        var dto = new ChangeEmailDto { UserId = Guid.NewGuid(), NewEmail = "new@example.com" };
        var responseObj = new UserAuthApiResponse<object> { Data = new object() };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.ChangeEmail(dto);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnSuccess_WhenResponseIsSuccessful()
    {
        var dto = new ChangePasswordDto { UserId = Guid.NewGuid(), NewPassword = "newpass", PreviousPassword = "oldpass" };
        var responseObj = new UserAuthApiResponse<object> { Data = new object() };
        var responseJson = JsonConvert.SerializeObject(responseObj);

        SetupHttpResponse(HttpStatusCode.OK, responseJson);

        var result = await _userService.ChangePassword(dto);

        Assert.True(result.IsSuccess);
    }
    [Fact]
    public async Task Register_ShouldReturnFailure_WhenApiReturnsError()
    {
        var dto = new RegisterUserDto { UserName = "test", Email = "test", Password = "123", Contacts = "abc" };
        var error = new ErrorDto { Error = "BadRequest", Details = "Invalid email", Status = 400 };
        var json = JsonConvert.SerializeObject(new UserAuthApiResponse<object> { Error = error });

        SetupHttpResponse(HttpStatusCode.BadRequest, json);

        var result = await _userService.Register(dto);

        Assert.True(result.IsFailure);
        Assert.Contains("BadRequest", result.Error);
    }

    [Fact]
    public async Task Login_ShouldReturnFailure_WhenCredentialsInvalid()
    {
        var dto = new LoginUserDto { Email = "fail@example.com", Password = "wrong" };
        var error = new ErrorDto { Error = "Unauthorized", Details = "Invalid credentials", Status = 401 };
        var json = JsonConvert.SerializeObject(new UserAuthApiResponse<LoginTokenDto> { Error = error });

        SetupHttpResponse(HttpStatusCode.Unauthorized, json);

        var result = await _userService.Login(dto);

        Assert.True(result.IsFailure);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task GetUserInfo_ShouldReturnFailure_WhenUserNotFound()
    {
        var error = new ErrorDto { Error = "NotFound", Details = "User not found", Status = 404 };
        var json = JsonConvert.SerializeObject(new UserAuthApiResponse<UserInfoDto> { Error = error });

        SetupHttpResponse(HttpStatusCode.NotFound, json);

        var result = await _userService.GetUserInfo(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Contains("User not found", result.Error);
    }

    [Fact]
    public async Task ChangeUserName_ShouldReturnFailure_WhenApiReturnsServerError()
    {
        var error = new ErrorDto { Error = "Server Error", Details = "Internal error", Status = 500 };
        var json = JsonConvert.SerializeObject(new UserAuthApiResponse<object> { Error = error });

        SetupHttpResponse(HttpStatusCode.InternalServerError, json);

        var result = await _userService.ChangeUserName(new ChangeUserNameDto
        {
            UserId = Guid.NewGuid(),
            NewUserName = "broken"
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Internal error", result.Error);
    }


    [Fact]
    public async Task ChangePassword_ShouldReturnFailure_WhenNoDataAndNoErrorProvided()
    {
        var json = JsonConvert.SerializeObject(new UserAuthApiResponse<object>());
        SetupHttpResponse(HttpStatusCode.InternalServerError, json);

        var result = await _userService.ChangePassword(new ChangePasswordDto
        {
            UserId = Guid.NewGuid(),
            NewPassword = "newpass",
            PreviousPassword = "wrongold"
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Server Error", result.Error);
    }
    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}
