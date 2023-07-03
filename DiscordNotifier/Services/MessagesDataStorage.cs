using StackExchange.Redis;

namespace DiscordNotifier.Services;

public class MessagesDataStorage
{
    private readonly IConnectionMultiplexer _redis;

    public MessagesDataStorage(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SaveDeleteMessageJobId(ulong channelId, string jobId)
    {
        var db = _redis.GetDatabase();

        await db.StringSetAsync($"job:delete-channel:{channelId}", jobId);
    }

    public async Task<string?> GetDeleteMessageJobId(ulong channelId)
    {
        var db = _redis.GetDatabase();
        var res = await db.StringGetAsync($"job:delete-channel:{channelId}");

        return res.HasValue
            ? res.ToString()
            : null;
    }

    public async Task<int?> GetChannelStateMessage(ulong channelId)
    {
        var db = _redis.GetDatabase();
        var v = await db.StringGetAsync($"msg:channel:{channelId}");

        return v.HasValue
            ? (int)v
            : null;
    }

    public async Task SetChannelStateMessage(ulong channelId, int? messageId)
    {
        var db = _redis.GetDatabase();
        
        await db.StringSetAsync($"msg:channel:{channelId}", messageId);
    }
}