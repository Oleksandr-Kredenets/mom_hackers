using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
using TMS.Application.Interfaces;
using TMS.Application.Services;
using TMS.Domain.Interfaces;
using TMS.Domain.Models;
using TMS.Infrastructure;
using TMS.Infrastructure.Repositories;
using TMS.Infrastructure.Routing;
using TMS.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var authSecret = builder.Configuration["AUTH_SECRET"];
if (string.IsNullOrWhiteSpace(authSecret))
    throw new InvalidOperationException(
        "AUTH_SECRET is required. Set the AUTH_SECRET environment variable.");
if (Encoding.UTF8.GetByteCount(authSecret) < 32)
    throw new InvalidOperationException(
        "AUTH_SECRET must be at least 32 UTF-8 bytes for HS256 signing.");

var valhallaUrl = builder.Configuration["VALHALLA_API_URL"]
                  ?? builder.Configuration["Valhalla:ApiUrl"];
if (string.IsNullOrWhiteSpace(valhallaUrl))
    throw new InvalidOperationException(
        "VALHALLA_API_URL (or Valhalla:ApiUrl) is required. Example: http://localhost:8002");
valhallaUrl = valhallaUrl.Trim().TrimEnd('/') + "/";

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseInMemoryDatabase("dev"));

builder.Services.AddSingleton<IJwtTokenService>(_ => new JwtTokenService(authSecret));
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IValhallaClient, ValhallaClient>(client =>
{
    client.BaseAddress = new Uri(valhallaUrl);
    client.Timeout = TimeSpan.FromMinutes(1);
});
builder.Services.AddScoped<IRoutePlanService, RoutePlanService>();

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

var corsOrigins = ResolveCorsOrigins(builder.Configuration);
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    });

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(options =>
// {
//     options.SwaggerDoc("v1", new OpenApiInfo { Title = "TMS API", Version = "v1" });
//     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme.",
//         Name = "Authorization",
//         In = ParameterLocation.Header,
//         Type = SecuritySchemeType.Http,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//     });
//     options.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer",
//                 },
//             },
//             Array.Empty<string>()
//         },
//     });
// });

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TMS API v1"));
// }

app.UseRouting();
if (corsOrigins.Length > 0)
    app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static string[] ResolveCorsOrigins(IConfiguration configuration)
{
    var fromEnv = configuration["CORS_ALLOWED_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(fromEnv))
    {
        return fromEnv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    return configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
}