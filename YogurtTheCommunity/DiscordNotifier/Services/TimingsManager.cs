using StackExchange.Redis;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class TimingsManager
{
    private readonly IConnectionMultiplexer _redis;

    public TimingsManager(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<DateTime> GetLastReportedJoinTime(ulong userId) => await GetTiming(userId, "joined");

    public async Task SetLastReportedJoinTime(ulong userId, DateTime time) => await SetLastTiming(userId, "joined", time);

    public async Task<DateTime> GetLastStreamingTime(ulong userId) => await GetTiming(userId, "streaming");

    public async Task SetLastStreamingTime(ulong userId, DateTime time) => await SetLastTiming(userId, "streaming", time);

    private async Task<DateTime> GetTiming(ulong userId, string name)
    {
        var db = _redis.GetDatabase();

        var val = await db.StringGetAsync($"community:discord-notifier:timings:{userId}:{name}");

        return val.HasValue
            ? new DateTime(ticks: (long)val)
            : DateTime.UnixEpoch;
    }

    public async Task SetLastTiming(ulong userId, string name, DateTime time)
    {
        var db = _redis.GetDatabase();

        await db.StringSetAsync($"community:discord-notifier:timings:{userId}:{name}", time.Ticks);
    }
}