using System.Data;
using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data.SqlEntities;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;
using Microsoft.EntityFrameworkCore;


namespace Auction.UserAuthService.Data.Repositories
{
    public class SqlUserRepository 
    {
        private readonly UserRolePermissionDbContext _context;

        public SqlUserRepository(UserRolePermissionDbContext context)
        {
            _context = context;
        }

        public async Task Add(User user)
        {
            if (user == null)
            {
                throw new ArgumentException(nameof(user), "User cannot be null");
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == user.Email.ToLower());

            if (emailExists)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var userEntity = new UserEntity
            {
                UserId = user.UserId,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
                Email = user.Email,
                Contacts = user.Contacts,
            };

            var userRoleEntity = new UserRoleEntity
            {
                UserId = user.UserId,
                RoleId = 2
            };

            await _context.UserRole.AddAsync(userRoleEntity);
            await _context.Users.AddAsync(userEntity);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> UserExistsByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }

            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (userEntity == null)
            {
                return null;
            }
            var user = User.Create(
                userEntity.UserId,
                userEntity.UserName,
                userEntity.PasswordHash,
                userEntity.Email,
                userEntity.Contacts
                );

            return user;
        }
        public async Task<List<string>> GetUsersPermissions(Guid userId)
        {
            if (userId == Guid.Empty) return new List<string>();

                var user = await _context.Users
                    .Include(u => u.Roles)
                        .ThenInclude(r => r.Permissions)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return new List<string>();

            return user.Roles
                .SelectMany(r => r.Permissions)
                .Select(p => p.Name)
                .Distinct()
                .ToList();
        }
        public async Task Update(User user)
        {
            if (user == null)
            {
                throw new ArgumentException(nameof(user), "User cannot be null");
            }
            var userExists = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (userExists != null)
            {
                userExists.UserName = user.UserName;
                userExists.Email = user.Email;
                userExists.PasswordHash = user.PasswordHash;
                userExists.Contacts = user.Contacts;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetByUserId(Guid userId)
        { var userEntity =  await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (userEntity == null) return null;
            var user = User.Create(
               userEntity.UserId,
               userEntity.UserName,
               userEntity.PasswordHash,
               userEntity.Email,
               userEntity.Contacts
               );
            return user;
        }
    }
}