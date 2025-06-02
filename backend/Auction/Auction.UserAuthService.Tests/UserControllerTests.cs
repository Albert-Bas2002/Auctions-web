using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auction.ApiGateway.Controllers;
using Auction.UserAuthService.Contracts;
using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Abstractions;
using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Moq;
using Xunit;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IValidator<RegisterUserDto>> _registerValidatorMock = new();
    private readonly Mock<IValidator<LoginUserDto>> _loginValidatorMock = new();
    private readonly Mock<IValidator<ChangePasswordDto>> _changePasswordValidatorMock = new();
    private readonly Mock<IValidator<ChangeUserNameDto>> _changeUserNameValidatorMock = new();
    private readonly Mock<IValidator<ChangeEmailDto>> _changeEmailValidatorMock = new();
    private readonly Mock<IValidator<ChangeContactsDto>> _changeContactsValidatorMock = new();
    private readonly Mock<IErrorMessageParser> _errorMessageParserMock = new();

    private UserController CreateController()
    {
        return new UserController(
            _userServiceMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object,
            _changePasswordValidatorMock.Object,
            _changeUserNameValidatorMock.Object,
            _changeEmailValidatorMock.Object,
            _changeContactsValidatorMock.Object,
            _errorMessageParserMock.Object
        );
    }

    private ValidationResult ValidValidationResult() => new ValidationResult();
    private ValidationResult InvalidValidationResult(params string[] errors) =>
        new ValidationResult(errors.Select(e => new ValidationFailure("", e)));

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        var dto = new RegisterUserDto();
        _registerValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(InvalidValidationResult("Error1", "Error2"));
        var controller = CreateController();

        // Act
        var result = await controller.Register(dto);

        // Assert
        var badRequest = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);

        var response = Assert.IsType<UserAuthApiResponse<object>>(badRequest.Value);
        Assert.NotNull(response.Error);
        Assert.Contains("Error1", response.Error.Details);
        Assert.Contains("Error2", response.Error.Details);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
    {
        var dto = new RegisterUserDto { UserName = "test", Email = "test@mail.com", Password = "pass", Contacts = "contact" };
        _registerValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(ValidValidationResult());
        _userServiceMock.Setup(s => s.Register(dto.UserName, dto.Email, dto.Password, dto.Contacts))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<object>>(okResult.Value);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenValidationFails()
    {
        var dto = new LoginUserDto();
        _loginValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(InvalidValidationResult("Invalid login"));

        var controller = CreateController();

        var result = await controller.Login(dto);

        var badRequest = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);

        var response = Assert.IsType<UserAuthApiResponse<LoginTokenDto>>(badRequest.Value);
        Assert.NotNull(response.Error);
        Assert.Contains("Invalid login", response.Error.Details);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSucceeds()
    {
        var dto = new LoginUserDto { Email = "test@mail.com", Password = "pass" };
        var token = new LoginTokenDto { Token = "jwt.token" };

        _loginValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(ValidValidationResult());
        _userServiceMock.Setup(s => s.Login(dto.Email, dto.Password)).ReturnsAsync(Result.Success(token));

        var controller = CreateController();

        var result = await controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<LoginTokenDto>>(okResult.Value);
        Assert.Null(response.Error);
        Assert.Equal("jwt.token", response.Data.Token);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsOk_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var userInfo = new UserInfoDto { UserName = "user", Email = "email@mail.com" };

        _userServiceMock.Setup(s => s.GetUserInfo(userId)).ReturnsAsync(Result.Success(userInfo));
        var controller = CreateController();

        var result = await controller.GetUserInfo(userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<UserInfoDto>>(okResult.Value);
        Assert.Null(response.Error);
        Assert.Equal(userInfo.UserName, response.Data.UserName);
    }

    [Fact]
    public async Task GetUserInfo_ReturnsError_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();
        var errorDto = new ErrorDto { Error = "Not found", Details = "User not found", Status = 404 };

        _userServiceMock.Setup(s => s.GetUserInfo(userId)).ReturnsAsync(Result.Failure<UserInfoDto>("User not found"));
        _errorMessageParserMock.Setup(p => p.ParseMessageToErrorDto("User not found")).Returns(errorDto);

        var controller = CreateController();

        var result = await controller.GetUserInfo(userId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, objectResult.StatusCode);

        var response = Assert.IsType<UserAuthApiResponse<UserInfoDto>>(objectResult.Value);
        Assert.NotNull(response.Error);
        Assert.Equal("Not found", response.Error.Error);
    }

    [Fact]
    public async Task IsUserByUserId_ReturnsTrueOrFalse()
    {
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(s => s.IsUserByUserId(userId)).ReturnsAsync(true);
        var controller = CreateController();

        var result = await controller.IsUserByUserId(userId);

        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(actionResult.Value);
    }
    [Fact]
    public async Task ChangeEmail_ReturnsOk_WhenChangeEmailSucceeds()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangeEmailDto { UserId = userId, NewEmail = "email@test.com" };

        _changeEmailValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult()); 

        _userServiceMock.Setup(s => s.ChangeEmail(userId, dto.NewEmail))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.ChangeEmail(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }
    [Fact]
    public async Task ChangePassword_ReturnsOk_WhenChangePasswordSucceeds()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto { UserId = userId, NewPassword = "fff",PreviousPassword="gggg" };

        _changePasswordValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userServiceMock.Setup(s => s.ChangePassword(userId, dto.NewPassword,dto.PreviousPassword))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.ChangePassword(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }
    [Fact]
    public async Task ChangeUserName_ReturnsOk_WhenChangeUserNameSucceeds()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangeUserNameDto { UserId = userId, NewUserName = "newusername" };

        _changeUserNameValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userServiceMock.Setup(s => s.ChangeUserName(userId, dto.NewUserName))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.ChangeUserName(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }
    [Fact]
    public async Task ChangeContacts_ReturnsOk_WhenChangeContactsSucceeds()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangeContactsDto { UserId = userId, NewContacts = "new contacts info" };

        _changeContactsValidatorMock.Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _userServiceMock.Setup(s => s.ChangeContacts(userId, dto.NewContacts))
            .ReturnsAsync(Result.Success());

        var controller = CreateController();

        var result = await controller.ChangeContacts(dto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserAuthApiResponse<object>>(okResult.Value);

        Assert.Null(response.Error);
    }


}
