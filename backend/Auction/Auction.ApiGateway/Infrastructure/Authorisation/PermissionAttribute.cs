
using Microsoft.AspNetCore.Authorization;

namespace Auction.ApiGateway.Infrastructure.Authorisation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : AuthorizeAttribute
    {
        public PermissionAttribute(string permission)
        {
            Policy = $"Permission:{permission}";
        }
    }
}
