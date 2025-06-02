using Auction.UserAuthService.Contracts;
using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Core.Validators;
using Auction.UserAuthService.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Auction.ApiGateway.Controllers
{
    [ApiController]
    [Route("api-users/")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordDtoValidator;
        private readonly IValidator<ChangeUserNameDto> _changeUserNameDtoValidator;
        private readonly IValidator<ChangeEmailDto> _changeEmailDtoValidator;
        private readonly IValidator<ChangeContactsDto> _changeContactsDtoValidator;

        private readonly IErrorMessageParser _errorMessageParser;


        public UserController(
            IUserService userService,
            IValidator<RegisterUserDto> registerValidator,
            IValidator<LoginUserDto> loginValidator,
            IValidator<ChangePasswordDto> changePasswordDtoValidator,
            IValidator<ChangeUserNameDto> changeUserNameDtoValidator,
            IValidator<ChangeEmailDto> changeEmailDtoValidator,
             IValidator<ChangeContactsDto> changeContactsDtoValidator,
            IErrorMessageParser errorMessageParser)
        {
            _userService = userService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _changePasswordDtoValidator = changePasswordDtoValidator;
            _changeUserNameDtoValidator = changeUserNameDtoValidator;
            _errorMessageParser = errorMessageParser;
            _changeEmailDtoValidator = changeEmailDtoValidator;
            _changeContactsDtoValidator = changeContactsDtoValidator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserAuthApiResponse<object>>> Register([FromBody] RegisterUserDto registerUserDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerUserDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }

            var registerResult = await _userService.Register(registerUserDto.UserName, registerUserDto.Email, registerUserDto.Password, registerUserDto.Contacts);
            if (registerResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(registerResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });

            }

        }





        [HttpPost("login")]
        public async Task<ActionResult<UserAuthApiResponse<LoginTokenDto>>> Login([FromBody] LoginUserDto loginUserDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginUserDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<LoginTokenDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }

            var loginResult = await _userService.Login(loginUserDto.Email, loginUserDto.Password);

            if (loginResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<LoginTokenDto> {
                    Data = loginResult.Value
                });
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(loginResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });

            }



        }
        [HttpGet("exists/{userId:guid}")]
        public async Task<ActionResult<bool>> IsUserByUserId( Guid userId)
        {
            return await _userService.IsUserByUserId(userId);
        }
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<UserAuthApiResponse<UserInfoDto>>> GetUserInfo(Guid userId)
        {
            var userInfoResult = await _userService.GetUserInfo(userId);
            if (userInfoResult.IsSuccess)
            {
                var userInfo = userInfoResult.Value;
                return Ok(new UserAuthApiResponse<UserInfoDto> {Data = userInfo});
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(userInfoResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<UserInfoDto>
                {
                    Error = error
                });
            }
        }
        [HttpPut("change-password")]
        public async Task<ActionResult<UserAuthApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var validationResult = await _changePasswordDtoValidator.ValidateAsync(changePasswordDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }
            var changePasswordResult = await _userService.ChangePassword(changePasswordDto.UserId, changePasswordDto.NewPassword, changePasswordDto.PreviousPassword);
            if (changePasswordResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<object>());
                
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changePasswordResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });

            }
        }
        [HttpPut("change-contacts")]
        public async Task<ActionResult<UserAuthApiResponse<object>>> ChangeContacts([FromBody] ChangeContactsDto contactDto)
        {
            var validationResult = await _changeContactsDtoValidator.ValidateAsync(contactDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }

            var changeContactResult = await _userService.ChangeContacts(contactDto.UserId, contactDto.NewContacts);
            if (changeContactResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changeContactResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });
            }
        }

        [HttpPut("change-username")]
        public async Task<ActionResult<UserAuthApiResponse<object>>> ChangeUserName([FromBody] ChangeUserNameDto userNameDto)
        {
            var validationResult = await _changeUserNameDtoValidator.ValidateAsync(userNameDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }
            var changeUserNameResult = await _userService.ChangeUserName(userNameDto.UserId, userNameDto.NewUserName);
            if (changeUserNameResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<object>()
                );
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changeUserNameResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });
            }

        }
        [HttpPut("change-email")]
        public async Task<ActionResult<UserAuthApiResponse<object>>> ChangeEmail([FromBody] ChangeEmailDto changeEmailDto)
        {
            var validationResult = await _changeEmailDtoValidator.ValidateAsync(changeEmailDto);

            if (!validationResult.IsValid)
            {
                return StatusCode(400, new UserAuthApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });
            }
            var changeEmailResult = await _userService.ChangeEmail(changeEmailDto.UserId, changeEmailDto.NewEmail);
            if (changeEmailResult.IsSuccess)
            {
                return Ok(new UserAuthApiResponse<object>()
                );
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changeEmailResult.Error);
                return StatusCode(error.Status, new UserAuthApiResponse<object>
                {
                    Error = error
                });
            }
        }
        }
}
