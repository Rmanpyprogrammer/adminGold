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
        TimeSpan ttl,
        int panel)
    {
        switch(panel)
        {
            default:
                await _db.StringSetAsync(
                    "digikala_krabo_access_token",
                    token,
                    ttl
                );
                break;
            case 1 :
                await _db.StringSetAsync(
                    "digikala_krabo_access_token",
                    token,
                    ttl
                );
                break;
            case 2 :
                await _db.StringSetAsync(
                    "digikala_fereshte_access_token",
                    token,
                    ttl
                );
                break;


        }

    }

    public async Task<string?> GetDigikalaTokenAsync(int panel)
    {

        var token = await _db.StringGetAsync(
            "digikala_krabo_access_token"
        );
        switch(panel)
        {

            case 1 :
                token = await _db.StringGetAsync(
                    "digikala_krabo_access_token"
                );
                break;
            case 2:
                token = await _db.StringGetAsync(
                    "digikala_fereshte_access_token"
                );         
                break;
        }


        return token.IsNullOrEmpty
            ? null
            : token.ToString();
    }

    public async Task<bool> HasTokenAsync(int panel)
    {
        switch(panel)
        {
            default:
                return await _db.KeyExistsAsync(
                    "digikala_krabo_access_token"
                );         
            case 1 :
                return await _db.KeyExistsAsync(
                    "digikala_krabo_access_token"
                );
            case 2:
                return await _db.KeyExistsAsync(
                    "digikala_fereshte_access_token"
                );
        }

    }
}