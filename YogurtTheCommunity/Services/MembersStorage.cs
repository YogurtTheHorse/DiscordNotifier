using Microsoft.Extensions.Options;
using StackExchange.Redis;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Options;

namespace YogurtTheCommunity.Services;

public class MembersStorage(
    IConnectionMultiplexer redis,
    ILogger<MembersStorage> logger,
    IOptions<MembersDefaultOptions> membersDefaultOptions)
{
    private const string NameField = "id";
    private const string TelegramIdField = "tg-id";

    public async Task<MemberInfo?> GetMemberByTelegramId(long telegramId)
    {
        var id = await GetIdFromTelegramId(telegramId);

        return await GetMemberById(id);
    }

    public async Task<MemberInfo?> GetMemberById(Guid id)
    {
        var db = redis.GetDatabase();

        var memberInfo = await db.HashGetAllAsync($"community:members:{id}");
        var roles = await db.SetMembersAsync($"community:members:{id}:roles");

        if (memberInfo.Length == 0)
        {
            return null;
        }

        return new MemberInfo(
            id,
            memberInfo.FirstOrDefault(x => x.Name == NameField).Value!,
            roles.Select(r => r.ToString()).ToArray()
        );
    }

    public async Task<ExtraMemberInfo> GetExtraInfo(Guid id)
    {
        var db = redis.GetDatabase();

        var extraInfoEntries = await db.HashGetAllAsync($"community:members:{id}:extra");

        return new ExtraMemberInfo(
            GetValue(nameof(ExtraMemberInfo.Description))?.ToString() ?? string.Empty
        );
        
        RedisValue? GetValue(string key)
        {
            var entry = extraInfoEntries.FirstOrDefault(e => e.Name == key);
            return entry.Value.IsNullOrEmpty
                ? (RedisValue?)null
                : entry.Value;
        }
    }
    
    public async Task SetExtraInfo(Guid id, ExtraMemberInfo extraInfo)
    {
        var db = redis.GetDatabase();

        await db.HashSetAsync($"community:members:{id}:extra",
        [
            new HashEntry(nameof(ExtraMemberInfo.Description), extraInfo.Description)
        ]);
    }

    public async Task SetName(Guid id, string name)
    {
        var db = redis.GetDatabase();

        await db.HashSetAsync($"community:members:{id}",
        [
            new HashEntry(NameField, name)
        ]);
    }

    public async Task AddRole(Guid memberId, string role)
    {
        var db = redis.GetDatabase();

        await db.SetAddAsync($"community:members:{memberId}:roles", role);
    }

    public async Task RemoveRole(Guid memberId, string role)
    {
        var db = redis.GetDatabase();

        await db.SetRemoveAsync($"community:members:{memberId}:roles", role);
    }

    public async Task<long?> GetTelegramId(Guid id)
    {
        var defaultId = membersDefaultOptions.Value.TelegramDefaultIds.FirstOrDefault(x => x.Value.DefaultId == id)
            .Key;

        if (defaultId != 0)
        {
            return defaultId;
        }

        var db = redis.GetDatabase();

        var tgId = await db.HashGetAsync($"community:members:{id}", TelegramIdField);

        return tgId.HasValue ? (long)tgId : null;
    }

    public async Task<Guid> GetIdFromTelegramId(long telegramId)
    {
        if (TryGetDefaultMemberFromTelegram(telegramId, out var defaultMember))
        {
            return defaultMember.DefaultId;
        }

        var db = redis.GetDatabase();
        var fromDb = await db.StringGetAsync(GetTelegramIdKey(telegramId));

        if (fromDb.HasValue)
        {
            return Guid.Parse(fromDb.ToString());
        }

        var newId = Guid.NewGuid();
        await db.StringSetAsync(GetTelegramIdKey(telegramId), newId.ToString());

        logger.LogInformation("Created new id for telegram id {telegramId}: {newId}", telegramId, newId);

        return newId;
    }

    public async Task<MemberInfo> CreateFromTelegram(long telegramId, string name)
    {
        var defaultMember = GetDefaultMemberFromTelegram(telegramId);

        var id = await GetIdFromTelegramId(telegramId);
        var roles = defaultMember?.DefaultRoles ?? membersDefaultOptions.Value.DefaultRoles;

        var db = redis.GetDatabase();

        await db.HashSetAsync($"community:members:{id}",
        [
            new HashEntry(NameField, name), new HashEntry(TelegramIdField, telegramId)
        ]);
        await db.SetAddAsync($"community:members:{id}:roles", roles.Select(r => (RedisValue)r).ToArray());

        return new MemberInfo(id, name, roles);
    }

    private static string GetTelegramIdKey(long telegramId) => $"community:members:telegram:{telegramId}";

    private bool TryGetDefaultMemberFromTelegram(long telegramId, out MembersDefaultOptions.DefaultMember defaultMember)
    {
        var res = membersDefaultOptions.Value.TelegramDefaultIds.TryGetValue(telegramId, out var dm);

        defaultMember = dm ?? new MembersDefaultOptions.DefaultMember();

        return res;
    }

    private MembersDefaultOptions.DefaultMember? GetDefaultMemberFromTelegram(long telegramId) =>
        TryGetDefaultMemberFromTelegram(telegramId, out var defaultMember)
            ? defaultMember
            : null;
}