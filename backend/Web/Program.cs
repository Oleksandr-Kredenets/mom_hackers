using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TMS.Application.Interfaces;
using TMS.Application.Services;
using TMS.Domain.Interfaces;
using TMS.Infrastructure.Contexts;
using TMS.Infrastructure.Repositories;
using TMS.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using TMS.Domain.Models;

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

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Content("<h1>Hello, mother fu*kers!</h1>", "text/html"));

app.MapPost("/api/auth/register", async (IUserService userService, RegisterRequest body) =>
{
    try
    {
        await userService.RegisterAsync(body.Name, body.Email, body.Password);
        return Results.Ok();
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
});

app.MapPost("/api/auth/login", async (IUserService userService, LoginRequest body) =>
{
    try
    {
        var auth = await userService.LoginAsync(body.Email, body.Password);
        return Results.Ok(auth);
    }
    catch (InvalidOperationException)
    {
        return Results.Unauthorized();
    }
});

string? FirstClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
{
    foreach (var claimType in claimTypes)
    {
        var value = principal.FindFirstValue(claimType);
        if (!string.IsNullOrEmpty(value))
            return value;
    }

    return null;
}

app.MapGet("/api/me", (ClaimsPrincipal principal) =>
{
    var idText = FirstClaimValue(
        principal,
        ClaimTypes.NameIdentifier,
        JwtRegisteredClaimNames.Sub,
        "sub");
    if (!Guid.TryParse(idText, out var id))
        return Results.Unauthorized();

    var email = FirstClaimValue(
        principal,
        ClaimTypes.Email,
        JwtRegisteredClaimNames.Email,
        "email") ?? "";

    var name = FirstClaimValue(
        principal,
        ClaimTypes.Name,
        JwtRegisteredClaimNames.UniqueName,
        "unique_name",
        "name") ?? "";

    return Results.Ok(new AuthUserDto(id, name, email));
}).RequireAuthorization();

app.Run();