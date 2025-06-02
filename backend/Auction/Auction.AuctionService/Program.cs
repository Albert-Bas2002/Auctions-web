using Auction.AuctionService.Application.Services;
using Auction.AuctionService.Contracts;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Data.Repositories;
using Auction.AuctionService.Extensions;
using Auction.UserService.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuctionCreateUpdateService, AuctionCreateUpdateService>();
builder.Services.AddScoped<IAuctionDetailsRepository, AuctionDetailsRepository>();
builder.Services.AddScoped<IAuctionGetDetailsService, AuctionGetDetailsService>();
builder.Services.AddScoped<IAuctionStatusRepository, AuctionStatusRepository>();
builder.Services.AddScoped<IAuctionStatusService, AuctionStatusService>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IUserCategoryService, UserCategoryService>();
builder.Services.AddHostedService<AuctionBackgroundService>();

builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuctionDbContext")));
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
app.UseAuthorization();

app.MapControllers();

app.Run();
