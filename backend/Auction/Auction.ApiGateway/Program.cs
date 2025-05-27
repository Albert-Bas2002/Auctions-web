using Microsoft.AspNetCore.Cors;
using Auction.ApiGateway.Core.Abstractions;
using Auction.ApiGateway.Application.Services;
using Auction.ApiGateway.Extensions;
using Microsoft.Extensions.Options;
using Auction.ApiGateway.Infrastructure.Authentication;
using Auction.ApiGateway.Infrastructure.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using Auction.ApiGateway.Infrastructure;


var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuctionService, AuctionService> ();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8083); 
    serverOptions.ListenAnyIP(5003); 
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IErrorMessageParser, ErrorMessageParser>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allow", policy =>
        policy.WithOrigins("http://192.168.0.10:3000", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddApiAuthentication(
    builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>());//bad


builder.Services.AddHttpClient("UserAuthService", client =>
{
    client.BaseAddress = new Uri("http://user-auth-service:8080");
});
builder.Services.AddHttpClient("AuctionService", client =>
{
    client.BaseAddress = new Uri("http://auction-service:8081");
});


//builder.Services.AddHttpClient("UserAuthService", client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5000");
//});
//builder.Services.AddHttpClient("AuctionService", client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5001");
//});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Allow");

app.UseAuthentication(); 
app.UseAuthorization();




app.MapControllers();

app.Run();

