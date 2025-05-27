using System.Net.Http;
using System.Text.Json;
using Auction.ApiGateway.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Auction.ApiGateway.Infrastructure.Authorisation
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {

        HttpClient _httpClient;
        public PermissionHandler(IHttpClientFactory httpClientFactory)
        {

            _httpClient = httpClientFactory.CreateClient("UserAuthService");
        }

        protected override async Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    PermissionRequirement requirement
     )
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "userId");
            var userPermissionsClaim = context.User.Claims.FirstOrDefault(c => c.Type == "userPermissions"); 

            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Fail();
                return;
            }
            var response = await _httpClient.GetAsync($"api-users/exists/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var isUser = JsonSerializer.Deserialize<bool>(json);
                if (isUser)
                {
                    if (userPermissionsClaim != null)
                    {
                        var userPermissions = userPermissionsClaim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        if (userPermissions.Contains(requirement.Permission))
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }
                }
            }
            context.Fail();
            return; 
        }

    }
}
