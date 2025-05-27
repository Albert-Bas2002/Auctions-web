
using Microsoft.AspNetCore.Authorization;

namespace Auction.ApiGateway.Infrastructure.Authorisation
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

}
