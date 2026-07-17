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
using Hangfire;
using Hangfire.PostgreSql;
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



//hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(
            builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();


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


builder.WebHost.UseUrls("http://0.0.0.0:8080");
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<DigikalaAuthService>();
builder.Services.AddHttpClient<DigikalaClient>();

builder.Services.AddScoped<DigikalaSyncService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DigikalaJobs>();
// builder.Services.AddHostedService<DigikalaTokenWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var app = builder.Build();
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("RestrictedCors", policy =>
//     {
//         policy
//             .WithOrigins("http://192.168.1.50:3000")
//             .AllowAnyHeader()
//             .AllowAnyMethod();
//     });
// });

// app.UseCors("RestrictedCors");



//middleware
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }


app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<DigikalaJobs>(
    "refresh-token",
    x => x.RefreshTokenKrabo(),
    "*/55 * * * *");

RecurringJob.AddOrUpdate<DigikalaJobs>(
    "refresh-token2",
    x => x.RefreshTokenFereshte(),
    "*/55 * * * *");

RecurringJob.AddOrUpdate<DigikalaJobs>(
    "invoice-sync",
    x => x.AddInvoicesKrabo(),
    Cron.Daily(1));

RecurringJob.AddOrUpdate<DigikalaJobs>(
    "package-sync",
    x => x.AddPackagesKrabo(),
    Cron.Daily(1));
     

RecurringJob.AddOrUpdate<DigikalaJobs>(
    "invoice2-sync",
    x => x.AddInvoicesFereshte(),
    Cron.Daily(1));

RecurringJob.AddOrUpdate<DigikalaJobs>(
    "package2-sync",
    x => x.AddPackagesFereshte(),
    Cron.Daily(1));
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    await DbSeeder.SeedAdminAsync(scope.ServiceProvider);

}

// app.UseCors("RestrictedCors");
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
    "Core API Running"
);


app.Run();
