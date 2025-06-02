using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Auction.UserAuthService.Data.MongoDocuments
{
    public class UserDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("contacts")]
        public string Contacts { get; set; } = string.Empty;

        [BsonElement("roles")]
        public List<RoleDocument> Roles { get; set; } = new();
    }
}
