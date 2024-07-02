using StackExchange.Redis;

namespace YogurtTheCommunity.JoinsManager;

public class JoinsStorage(IConnectionMultiplexer redis)
{
    public async Task<bool> IsApproved(Guid memberId)
    {
        var db = redis.GetDatabase();

        return await db.KeyExistsAsync($"community:joins:{memberId}");
    }

    public async Task Approve(Guid memberId)
    {
        var db = redis.GetDatabase();

        await db.StringSetAsync($"community:joins:{memberId}", true);
    }

    public async Task AddJoinRequest(Guid memberId, long chatId)
    {
        var db = redis.GetDatabase();

        await db.SetAddAsync($"community:joins:requests:{memberId}", chatId);
    }
    
    public async Task<long[]> GetJoinRequests(Guid memberId, bool flush)
    {
        var db = redis.GetDatabase();

        var chats = (await db.SetMembersAsync($"community:joins:requests:{memberId}")).Select(c => (long)c).ToArray();
        
        if (flush)
        {
            await db.KeyDeleteAsync($"community:joins:requests:{memberId}");
        }

        return chats;
    }
}