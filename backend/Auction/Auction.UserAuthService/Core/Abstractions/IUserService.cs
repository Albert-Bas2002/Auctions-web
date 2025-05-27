using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Models;
using CSharpFunctionalExtensions;

namespace Auction.UserAuthService.Core.Abstractions
{
    public interface IUserService
    {
        Task<Result> ChangePassword(Guid userId, string newPassword, string previousPassword);
        Task<Result> ChangeUserName(Guid userId, string newUserName);
        Task<bool> IsUserByUserId(Guid userId);
        Task<Result<LoginTokenDto>> Login(string email, string password);
        Task<Result> Register(string userName, string email, string password, string contacts);
        Task<Result<UserInfoDto>> GetUserInfo(Guid userId);
        Task<Result> ChangeEmail(Guid userId, string newEmail);
        Task<Result> ChangeContacts(Guid userId, string newContacts);
    }
}