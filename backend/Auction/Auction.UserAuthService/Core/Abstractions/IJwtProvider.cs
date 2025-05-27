using Auction.UserAuthService.Core.Models;

namespace Auction.UserAuthService.Core.Abstractions;

public interface IJwtProvider
{
    public string GenerateToken(User user, List<string> permissions);
}