using StackExchange.Redis;

namespace Core.API.Services.Cache;

public class RedisService
{
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetDigikalaTokenAsync(
        string token,
        TimeSpan ttl)
    {
        await _db.StringSetAsync(
            "digikala_access_token",
            token,
            ttl
        );
    }

    public async Task<string?> GetDigikalaTokenAsync()
    {
        var token = await _db.StringGetAsync(
            "digikala_access_token"
        );

        return token.IsNullOrEmpty
            ? null
            : token.ToString();
    }

    public async Task<bool> HasTokenAsync()
    {
        return await _db.KeyExistsAsync(
            "digikala_access_token"
        );
    }
}