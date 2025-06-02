using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Core.Models;
using Auction.UserAuthService.Data.Repositories;


public class UserRepository : IUserRepository
{
    private readonly SqlUserRepository? _sqlRepo;
    private readonly MongoUserRepository? _mongoRepo;
    private readonly RepositoryType _repositoryType;

    public UserRepository(
        SqlUserRepository? sqlRepo,
        MongoUserRepository? mongoRepo,
        RepositoryType repositoryType)
    {
        _sqlRepo = sqlRepo;
        _mongoRepo = mongoRepo;
        _repositoryType = repositoryType;
    }

    private Task<T> ChooseRepo<T>(Func<MongoUserRepository, Task<T>> mongoFunc, Func<SqlUserRepository, Task<T>> sqlFunc)
    {
        return _repositoryType switch
        {
            RepositoryType.Sql when _sqlRepo != null => sqlFunc(_sqlRepo),
            RepositoryType.Mongo when _mongoRepo != null => mongoFunc(_mongoRepo),
            _ => throw new InvalidOperationException("No repository configured or repository is null.")
        };
    }

    private Task ChooseRepo(Func<MongoUserRepository, Task> mongoFunc, Func<SqlUserRepository, Task> sqlFunc)
    {
        return _repositoryType switch
        {
            RepositoryType.Sql when _sqlRepo != null => sqlFunc(_sqlRepo),
            RepositoryType.Mongo when _mongoRepo != null => mongoFunc(_mongoRepo),
            _ => throw new InvalidOperationException("No repository configured or repository is null.")
        };
    }

    public Task<User?> GetByEmail(string email) =>
        ChooseRepo(mongo => mongo.GetByEmail(email), sql => sql.GetByEmail(email));

    public Task<User?> GetByUserId(Guid id) =>
        ChooseRepo(mongo => mongo.GetByUserId(id), sql => sql.GetByUserId(id));

    public Task Add(User user) =>
        ChooseRepo(mongo => mongo.Add(user), sql => sql.Add(user));

    public Task Update(User user) =>
        ChooseRepo(mongo => mongo.Update(user), sql => sql.Update(user));

    public Task<bool> UserExistsByEmail(string email) =>
        ChooseRepo(mongo => mongo.UserExistsByEmail(email), sql => sql.UserExistsByEmail(email));

    public Task<List<string>> GetUsersPermissions(Guid userId) =>
        ChooseRepo(mongo => mongo.GetUsersPermissions(userId), sql => sql.GetUsersPermissions(userId));
}
