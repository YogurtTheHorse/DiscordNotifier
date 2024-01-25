using StackExchange.Redis;

namespace YogurtTheCommunity.TitleGiver;

public class TitlesStorage
{
    private readonly IConnectionMultiplexer _redis;

    public TitlesStorage(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetTitle(Guid id, string title)
    {
        var db = _redis.GetDatabase();
        
        await db.StringSetAsync($"community:titles:{id}", title);
    }

    public async Task<string?> GetTitle(Guid id)
    {
        var db = _redis.GetDatabase();

        return await db.StringGetAsync($"community:titles:{id}");
    }
}