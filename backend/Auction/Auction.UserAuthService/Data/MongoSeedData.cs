using Auction.UserAuthService.Data.MongoDocuments;
using MongoDB.Driver;

namespace Auction.UserAuthService.Data
{
    public static class MongoSeedData
    {
        public static async Task SeedAsync(IMongoDatabase database)
        {
            var rolesCollection = database.GetCollection<RoleDocument>("Roles");
            var permissionsCollection = database.GetCollection<PermissionDocument>("Permissions");

            var rolesCount = await rolesCollection.CountDocumentsAsync(FilterDefinition<RoleDocument>.Empty);
            var permissionsCount = await permissionsCollection.CountDocumentsAsync(FilterDefinition<PermissionDocument>.Empty);

            if (permissionsCount == 0)
            {
                var permissions = new List<PermissionDocument>
            {
                new PermissionDocument { Name = "Moderator-Permission" },
                new PermissionDocument { Name = "User-Permission" }
            };
                await permissionsCollection.InsertManyAsync(permissions);
            }

            if (rolesCount == 0)
            {
                var moderatorPermission = await permissionsCollection.Find(p => p.Name == "Moderator-Permission").FirstOrDefaultAsync();
                var userPermission = await permissionsCollection.Find(p => p.Name == "User-Permission").FirstOrDefaultAsync();

                var roles = new List<RoleDocument>
            {
                new RoleDocument
                {
                    Name = "Moderator",
                    Permissions = new List<PermissionDocument> { moderatorPermission }
                },
                new RoleDocument
                {
                    Name = "CommonUser",
                    Permissions = new List<PermissionDocument> { userPermission }
                }
            };
                await rolesCollection.InsertManyAsync(roles);
            }
        }
    }

}
