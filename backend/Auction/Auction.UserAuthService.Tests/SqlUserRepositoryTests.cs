using System;
using System.Threading.Tasks;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data;
using Auction.UserAuthService.Data.SqlEntities.AuthEntities;
using Auction.UserAuthService.Data.SqlEntities;
using Auction.UserAuthService.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SqlUserRepositoryTests
{
    private DbContextOptions<UserRolePermissionDbContext> _dbContextOptions;

    public SqlUserRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UserRolePermissionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Add_And_GetByEmail_WorksCorrectly()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var user = User.Create(Guid.NewGuid(), "testuser", "hashedpass", "test@example.com", "123456");

        await repository.Add(user);
        var retrieved = await repository.GetByEmail("test@example.com");

        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
        Assert.Equal(user.UserName, retrieved.UserName);
        Assert.Equal(user.Contacts, retrieved.Contacts);
    }

    [Fact]
    public async Task UserExistsByEmail_ReturnsTrue_WhenUserExists()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var user = User.Create(Guid.NewGuid(), "user2", "pass", "exists@example.com", "contact");
        await repository.Add(user);

        var exists = await repository.UserExistsByEmail("exists@example.com");

        Assert.True(exists);
    }

    [Fact]
    public async Task GetByUserId_ReturnsUser_WhenExists()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var user = User.Create(Guid.NewGuid(), "byId", "pass", "byid@example.com", "cont");
        await repository.Add(user);

        var result = await repository.GetByUserId(user.UserId);

        Assert.NotNull(result);
        Assert.Equal("byId", result.UserName);
    }
    [Fact]
    public async Task Add_ThrowsException_WhenUserWithSameEmailExists()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var user1 = User.Create(Guid.NewGuid(), "user1", "hash", "duplicate@example.com", "contact");
        var user2 = User.Create(Guid.NewGuid(), "user2", "hash", "duplicate@example.com", "contact");

        await repository.Add(user1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.Add(user2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UserExistsByEmail_ThrowsArgumentException_WhenEmailIsInvalid(string invalidEmail)
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repository.UserExistsByEmail(invalidEmail));
    }

    [Fact]
    public async Task GetByEmail_ReturnsNull_WhenUserNotFound()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var result = await repository.GetByEmail("notfound@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ChangesUserFields()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var user = User.Create(Guid.NewGuid(), "original", "pass", "original@example.com", "contact");
        await repository.Add(user);

        var updatedUser = User.Create(user.UserId, "updated", "newpass", "updated@example.com", "newcontact");

        await repository.Update(updatedUser);

        var retrieved = await repository.GetByUserId(user.UserId);
        Assert.Equal("updated", retrieved.UserName);
        Assert.Equal("updated@example.com", retrieved.Email);
        Assert.Equal("newpass", retrieved.PasswordHash);
        Assert.Equal("newcontact", retrieved.Contacts);
    }

    [Fact]
    public async Task GetUsersPermissions_ReturnsEmptyList_WhenUserNotFound()
    {
        var context = new UserRolePermissionDbContext(_dbContextOptions);
        var repository = new SqlUserRepository(context);

        var result = await repository.GetUsersPermissions(Guid.NewGuid());

        Assert.Empty(result);
    }


    [Fact]
    public async Task GetUsersPermissions_ReturnsDistinctPermissions_WhenUserHasRoles_ManualSeed()
    {
        var options = new DbContextOptionsBuilder<UserRolePermissionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new UserRolePermissionDbContext(options);

        context.Permissions.AddRange(
            new PermissionEntity { Id = 1, Name = "Moderator-Permission" },
            new PermissionEntity { Id = 2, Name = "User-Permission" }
        );

        context.Roles.AddRange(
            new RoleEntity { Id = 1, Name = "Moderator" },
            new RoleEntity { Id = 2, Name = "CommonUser" }
        );

        context.RolePermission.AddRange(
            new RolePermissionEntity { RoleId = 1, PermissionId = 1 },
            new RolePermissionEntity { RoleId = 2, PermissionId = 2 }
        );

        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            UserId = userId,
            UserName = "permuser",
            Email = "perm@example.com",
            PasswordHash = "pass",
            Contacts = "contact",
        };
        context.Users.Add(userEntity);

        context.UserRole.Add(new UserRoleEntity
        {
            UserId = userId,
            RoleId = 2 
        });

        await context.SaveChangesAsync();

        var repository = new SqlUserRepository(context);
        var permissions = await repository.GetUsersPermissions(userId);

        Assert.Contains("User-Permission", permissions);
        Assert.Single(permissions);
    }



}
