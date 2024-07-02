using StackExchange.Redis;

namespace YogurtTheCommunity.Services;

public class ChatsRegistry(IConnectionMultiplexer redis)
{
    public async Task<bool> RegisterTelegramChat(long chatId)
    {
        var db = redis.GetDatabase();

        return await db.SetAddAsync("community:chats:telegram", chatId);
    }

    public async Task<long[]> GetManagedTelegramChats()
    {
        var db = redis.GetDatabase();

        return (await db.SetMembersAsync("community:chats:telegram")).Select(c => (long)c).ToArray();
    }

    public async Task<bool> IsTelegramChatRegistered(long chatId)
    {
        var db = redis.GetDatabase();

        return await db.SetContainsAsync("community:chats:telegram", chatId);
    }
}