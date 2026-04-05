using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TMS.Application.Interfaces;
using TMS.Application.Services;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure.Contexts;
using TMS.Infrastructure.Repositories;
using TMS.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var authSecret = builder.Configuration["AUTH_SECRET"];
if (string.IsNullOrWhiteSpace(authSecret))
    throw new InvalidOperationException(
        "AUTH_SECRET is required. Set the AUTH_SECRET environment variable.");
if (Encoding.UTF8.GetByteCount(authSecret) < 32)
    throw new InvalidOperationException(
        "AUTH_SECRET must be at least 32 UTF-8 bytes for HS256 signing.");

builder.Services.AddDbContext<TMSDbContext>(options =>
    options.UseInMemoryDatabase("dev"));

builder.Services.AddSingleton<IJwtTokenService>(_ => new JwtTokenService(authSecret));
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
