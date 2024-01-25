using Telegram.Bot.Types;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Utils;

public static class MembersUtils
{
    public static async Task<MemberInfo> GetOrCreate(this MembersStorage members, User user)
    {
        
        var name = $"{user.FirstName} {user.LastName}".Trim();

        return await members.GetMemberByTelegramId(user.Id)
               ?? await members.CreateFromTelegram(user.Id, name);
    }
}