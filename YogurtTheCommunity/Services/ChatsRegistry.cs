using StackExchange.Redis;

namespace YogurtTheCommunity.Services;

public class ChatsRegistry
{
    private readonly IConnectionMultiplexer _redis;

    public ChatsRegistry(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> RegisterTelegramChat(long chatId)
    {
        var db = _redis.GetDatabase();

        return await db.SetAddAsync("community:chats:telegram", chatId);
    }

    public async Task<long[]> GetManagedTelegramChats()
    {
        var db = _redis.GetDatabase();

        return (await db.SetMembersAsync("community:chats:telegram")).Select(c => (long)c).ToArray();
    }

    public async Task<bool> IsTelegramChatRegistered(long chatId)
    {
        var db = _redis.GetDatabase();

        return await db.SetContainsAsync("community:chats:telegram", chatId);
    }
}