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

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UserRolePermissionDbContext>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
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
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
