using Auction.HubService.Application.Hubs;
using Auction.HubService.Application.Services;
using Auction.HubService.Authentication;
using Auction.HubService.Core.Abstractions;
using Auction.HubService.Data;
using Auction.HubService.Data.Repositories;
using Auction.HubService.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Logging.AddConsole();
builder.Services.AddSignalR();
builder.Services.AddScoped<IAuctionHubService, AuctionHubService>();
builder.Services.AddScoped<IAuctionChatMessageRepository, AuctionChatMessageRepository>();
builder.Services.AddDbContext<AuctionChatDbContext>();

builder.Services.AddApiAuthentication(
    builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>());//bad
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8085);
    serverOptions.ListenAnyIP(5005);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow", policy =>
        policy.WithOrigins("http://192.168.0.10:3000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

//builder.Services.AddHttpClient("AuctionService", client =>
//{
//    client.BaseAddress = new Uri("http://auction-service:8081");
//});
builder.Services.AddHttpClient("AuctionService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});
var app = builder.Build();
app.UseCors("Allow");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<AuctionHub>("/auctionHub");


app.MapControllers();

app.Run();
