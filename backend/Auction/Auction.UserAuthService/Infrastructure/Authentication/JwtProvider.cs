
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Core.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Auction.UserAuthService.Infrastructure.Authentication;

namespace Auction.ApiGateway.Infrastructure.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;

        public JwtProvider(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }
       

        public string GenerateToken(User user, List<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim("userId", user.UserId.ToString()),
                new Claim("userPermissions", string.Join(",", permissions)) 
            };
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(signingCredentials: signingCredentials,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_options.ExpiresHours));
            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenValue;
        }
    }
}
