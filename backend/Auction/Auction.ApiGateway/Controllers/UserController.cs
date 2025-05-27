using System.IdentityModel.Tokens.Jwt;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using Auction.ApiGateway.Core.Abstractions;
using Auction.ApiGateway.Infrastructure.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.ApiGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IErrorMessageParser _errorMessageParser;
        public UserController(IUserService userService, IErrorMessageParser errorMessageParser)
        {
            _userService = userService;
            _errorMessageParser = errorMessageParser;
        }

        [HttpGet("user-info/{userId:guid}")]
        public async Task<ActionResult<ApiGatewayResponse<UserInfoDto>>> UserInfo(Guid userId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            
            var userInfoResult = await _userService.GetUserInfo(userId);
            
            if (userInfoResult.IsSuccess)
            {
                var userInfoDto = userInfoResult.Value;
                if (userIdClaim!= userId.ToString())
                {
                    userInfoDto.Email = "Hidden@hidden.com";
                }
                return Ok(new ApiGatewayResponse<UserInfoDto> { Data= userInfoDto });
            }
       
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(userInfoResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<UserInfoDto> { Error = error });

            }


        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> Register([FromBody] RegisterUserRequest request)
        {
            var registerUserDto = new RegisterUserDto
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = request.Password,
                Contacts = request.Contacts,
            };

            var registerResponse = await _userService.Register(registerUserDto);
            if (registerResponse.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(registerResponse.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error});

            }


        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> Login([FromBody] LoginUserRequest request)
        {
            var loginUserDto = new LoginUserDto
            {
                Email = request.Email,
                Password = request.Password,
            };

            var loginResult= await _userService.Login(loginUserDto);
            if (loginResult.IsSuccess)
            {
                HttpContext.Response.Cookies.Append("MyCookies", loginResult.Value.Token!, new CookieOptions
                {
                   
                });


                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(loginResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

            }
        }

        [Authorize]
        [Permission("User-Permission")]
        [HttpPut("change-username")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> ChangeUserName([FromBody] ChangeUserNameRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });

            var changeUserNameDto = new ChangeUserNameDto
            {
                UserId = Guid.Parse(userId),
                NewUserName = request.NewUserName,
            };

            var registerResponse = await _userService.ChangeUserName(changeUserNameDto);
            if (registerResponse.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(registerResponse.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });
            }
        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPut("change-email")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> ChangeEmail([FromBody] ChangeEmailRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });

            var changeEmailDto = new ChangeEmailDto
            {
                UserId = Guid.Parse(userId),
                NewEmail = request.NewEmail,
            };

            var changeEmailResponse = await _userService.ChangeEmail(changeEmailDto);
            if (changeEmailResponse.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changeEmailResponse.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });
            }
        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPut("change-contacts")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> ChangeContacts([FromBody] ChangeContactsRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });

            var changeContactsDto = new ChangeContactsDto
            {
                UserId = Guid.Parse(userId),
                NewContacts = request.NewContacts,
            };

            var changeContactsResponse = await _userService.ChangeContacts(changeContactsDto);
            if (changeContactsResponse.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changeContactsResponse.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });
            }
        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPut("change-password")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });

            var changePasswordDto = new ChangePasswordDto
            {
                UserId = Guid.Parse(userId),
                NewPassword = request.NewPassword,
                PreviousPassword = request.PreviousPassword,
            };

            var changePasswordResponse = await _userService.ChangePassword(changePasswordDto);
            if (changePasswordResponse.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(changePasswordResponse.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });
            }
        }

        [HttpGet("token-validation")]
        public ActionResult<ApiGatewayResponse<bool>> IsTokenValid()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new ApiGatewayResponse<bool>
                {
                    Data = true 
                });
            }

            return Ok(new ApiGatewayResponse<bool>
            {
                Data = false 
            });
        }

    }

}

