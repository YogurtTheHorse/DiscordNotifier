using StackExchange.Redis;

namespace YogurtTheCommunity.TitleGiver;

public class TitlesStorage(IConnectionMultiplexer redis)
{
    public async Task SetTitle(Guid id, string title)
    {
        var db = redis.GetDatabase();
        
        await db.StringSetAsync($"community:titles:{id}", title);
    }

    public async Task<string?> GetTitle(Guid id)
    {
        var db = redis.GetDatabase();

        return await db.StringGetAsync($"community:titles:{id}");
    }
}