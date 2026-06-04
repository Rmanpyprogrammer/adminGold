using Core.API.Configurations;
using Core.API.Data;
using Core.API.Models;
using Core.API.Services;
using Core.API.Services.Cache;
using Core.API.Services.Background;
using Core.API.Services.Digikala;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DigikalaSettings>(
    builder.Configuration.GetSection("Digikala")
);

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration
            .GetConnectionString("DefaultConnection")
    );
});

// Redis
var redis = builder.Configuration
    .GetSection("Redis");

builder.Services
    .AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration =
        $"{redis["Host"]}:{redis["Port"]}," +
        $"user={redis["User"]}," +
        $"password={redis["Password"]}";

    return ConnectionMultiplexer.Connect(
        configuration
    );
});

// Identity
builder.Services
    .AddIdentity<AppUser, AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<RedisService>();

// JWT
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwt =
            builder.Configuration
                .GetSection("Jwt");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwt["Key"]!
                        )
                    )
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient<DigikalaAuthService>();
builder.Services.AddHttpClient<DigikalaClient>();

builder.Services.AddScoped<DigikalaSyncService>();
builder.Services.AddHostedService<DigikalaTokenWorker>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
    "Core API Running"
);

using (var scope =
       app.Services.CreateScope())
{
    await DbSeeder
        .SeedAdminAsync(
            scope.ServiceProvider
        );
}

app.Run();