using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data.MongoDocuments;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auction.UserAuthService.Data.Repositories
{
    public class MongoUserRepository
    {
        private readonly IMongoCollection<UserDocument> _usersCollection;

        public MongoUserRepository(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<UserDocument>("Users");
        }

        public async Task Add(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var emailExists = await _usersCollection
                .Find(u => u.Email.ToLower() == user.Email.ToLower())
                .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("A user with this email already exists.");

            var userDoc = new UserDocument
            {
                UserId = user.UserId,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
                Email = user.Email,
                Contacts = user.Contacts,
                Roles = new List<RoleDocument>
                {
                    new RoleDocument
                    {
                        Name = "CommonUser",
                        Permissions = new List<PermissionDocument>
                        {
                            new PermissionDocument { Name = "User-Permission" }
                        }
                    }
                }
            };

            await _usersCollection.InsertOneAsync(userDoc);
        }

        public async Task<bool> UserExistsByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            return await _usersCollection
                .Find(u => u.Email.ToLower() == email.ToLower())
                .AnyAsync();
        }

        public async Task<User?> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var userDoc = await _usersCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();

            if (userDoc == null) return null;

            return MapToDomain(userDoc);
        }

        public async Task<List<string>> GetUsersPermissions(Guid userId)
        {
            var userDoc = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (userDoc == null) return new List<string>();

            return userDoc.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList();
        }

        public async Task Update(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var filter = Builders<UserDocument>.Filter.Eq(u => u.UserId, user.UserId);
            var update = Builders<UserDocument>.Update
                .Set(u => u.UserName, user.UserName)
                .Set(u => u.Email, user.Email)
                .Set(u => u.PasswordHash, user.PasswordHash)
                .Set(u => u.Contacts, user.Contacts);

            await _usersCollection.UpdateOneAsync(filter, update);
        }

        public async Task<User?> GetByUserId(Guid userId)
        {
            var userDoc = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (userDoc == null) return null;

            return MapToDomain(userDoc);
        }

        private User MapToDomain(UserDocument doc)
        {
            return User.Create(
                doc.UserId,
                doc.UserName,
                doc.PasswordHash,
                doc.Email,
                doc.Contacts
            );
        }
    }
}
