using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auction.UserAuthService.Data.MongoDocuments
{
    public class RoleDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("permissions")]
        public List<PermissionDocument> Permissions { get; set; } = new();
    }
}
