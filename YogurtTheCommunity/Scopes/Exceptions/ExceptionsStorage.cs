using StackExchange.Redis;

namespace YogurtTheCommunity.Scopes.Exceptions;

public class ExceptionsStorage(IConnectionMultiplexer redis)
{
    public async Task AddExceptionForTelegramChat(string exception, long chatId)
    {
        var db = redis.GetDatabase();

        await db.SetAddAsync($"community:exceptions:tg:{chatId}", exception);
    }

    public async Task RemoveExceptionFromTelegramChat(string exception, long chatId)
    {
        var db = redis.GetDatabase();

        await db.SetRemoveAsync($"community:exceptions:tg:{chatId}", exception);
    }

    public async Task<IReadOnlyList<string>> GetExceptionsForTelegramChatId(long chatId)
    {
        var db = redis.GetDatabase();

        return (await db.SetMembersAsync($"community:exceptions:tg:{chatId}"))
            .Select(e => e.ToString())
            .ToArray();
    }

    public async Task<bool> HasException(long tgChatId, string exception)
    {
        var db = redis.GetDatabase();

        return await db.SetContainsAsync($"community:exceptions:tg:{tgChatId}", exception);
    }
}