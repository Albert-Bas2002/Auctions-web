using Microsoft.EntityFrameworkCore;
using Auction.UserAuthService.Data.Repositories;
using Microsoft.AspNetCore.Cors;
using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Application.Services;
using Auction.UserAuthService.Infrastructure.Authentication;
using Auction.UserAuthService.Data;
using Auction.UserAuthService.Infrastructure;
using Auction.ApiGateway.Infrastructure.Authentication;
using Auction.AuctionService.Extensions;
using Auction.UserAuthService.Core.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using static CSharpFunctionalExtensions.Result;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserRolePermissionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(UserRolePermissionDbContext))));

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoSettings>>().Value;
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(settings.DatabaseName);
});

var repositoryType = builder.Configuration.GetValue<string>("RepositoryType");

if (repositoryType == "Sql")
{
    builder.Services.AddScoped<SqlUserRepository>();

    builder.Services.AddScoped<UserRepository>(sp =>
    {
        var sqlRepo = sp.GetRequiredService<SqlUserRepository>();
        return new UserRepository(sqlRepo, null, RepositoryType.Sql);
    });
}
else if (repositoryType == "Mongo")
{
    builder.Services.AddScoped<MongoUserRepository>();

    builder.Services.AddScoped<UserRepository>(sp =>
    {
        var mongoRepo = sp.GetRequiredService<MongoUserRepository>();
        return new UserRepository(null, mongoRepo, RepositoryType.Mongo);
    });
}
else
{
    throw new Exception("Unknown RepositoryType in configuration");
}

builder.Services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IErrorMessageParser, ErrorMessageParser>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginUserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserDtoValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Logging.AddConsole();

var app = builder.Build();

if (repositoryType == "Mongo")
{
    using (var scope = app.Services.CreateScope())
    {
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        await MongoSeedData.SeedAsync(database);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
