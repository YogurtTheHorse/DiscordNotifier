using StackExchange.Redis;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class MessagesDataStorage(IConnectionMultiplexer redis)
{
    public async Task SaveDeleteMessageJobId(ulong channelId, string jobId)
    {
        var db = redis.GetDatabase();

        await db.StringSetAsync($"community:discord-notifier:job:delete-channel:{channelId}", jobId);
    }

    public async Task<string?> GetDeleteMessageJobId(ulong channelId)
    {
        var db = redis.GetDatabase();
        var res = await db.StringGetAsync($"community:discord-notifier:job:delete-channel:{channelId}");

        return res.HasValue
            ? res.ToString()
            : null;
    }

    public async Task<int?> GetChannelStateMessage(ulong channelId)
    {
        var db = redis.GetDatabase();
        var v = await db.StringGetAsync($"community:discord-notifier:msg:channel:{channelId}");

        return v.HasValue
            ? (int)v
            : null;
    }

    public async Task SetChannelStateMessage(ulong channelId, int? messageId)
    {
        var db = redis.GetDatabase();
        
        await db.StringSetAsync($"community:discord-notifier:msg:channel:{channelId}", messageId);
    }
}