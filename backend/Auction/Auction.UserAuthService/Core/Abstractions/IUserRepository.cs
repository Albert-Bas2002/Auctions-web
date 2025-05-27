using Auction.UserAuthService.Core.Models;

namespace Auction.UserAuthService.Core.Abstractions
{
    public interface IUserRepository
    {
        Task Add(User user);
        Task<User?> GetByEmail(string email);
       Task<List<string>> GetUsersPermissions(Guid userId);
        Task<bool> UserExistsByEmail(string email);
        Task Update(User user);
        Task<User?> GetByUserId(Guid userId);
    }
}