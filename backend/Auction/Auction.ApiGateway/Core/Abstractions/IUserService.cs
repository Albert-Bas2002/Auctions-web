using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using CSharpFunctionalExtensions;

namespace Auction.ApiGateway.Core.Abstractions
{
    public interface IUserService
    {
        Task<Result> ChangePassword(ChangePasswordDto changePasswordDto);
        Task<Result> ChangeUserName(ChangeUserNameDto changeUserNameDto);
        Task<Result<LoginTokenDto>> Login(LoginUserDto loginUserDto);
        Task<Result> Register(RegisterUserDto registerUserDto);
        Task<Result<UserInfoDto>> GetUserInfo(Guid userId);
        Task<Result> ChangeEmail(ChangeEmailDto changeEmailDto);
        Task<Result> ChangeContacts(ChangeContactsDto changeContactsDto);
    }
}