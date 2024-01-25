using StackExchange.Redis;

namespace YogurtTheCommunity.JoinsManager;

public class JoinsStorage
{
    private readonly IConnectionMultiplexer _redis;

    public JoinsStorage(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    public async Task<bool> IsApproved(Guid memberId)
    {
        var db = _redis.GetDatabase();

        return await db.KeyExistsAsync($"community:joins:{memberId}");
    }

    public async Task Approve(Guid memberId)
    {
        var db = _redis.GetDatabase();

        await db.StringSetAsync($"community:joins:{memberId}", true);
    }

    public async Task AddJoinRequest(Guid memberId, long chatId)
    {
        var db = _redis.GetDatabase();

        await db.SetAddAsync($"community:joins:requests:{memberId}", chatId);
    }
    
    public async Task<long[]> GetJoinRequests(Guid memberId, bool flush)
    {
        var db = _redis.GetDatabase();

        var chats = (await db.SetMembersAsync($"community:joins:requests:{memberId}")).Select(c => (long)c).ToArray();
        
        if (flush)
        {
            await db.KeyDeleteAsync($"community:joins:requests:{memberId}");
        }

        return chats;
    }
}