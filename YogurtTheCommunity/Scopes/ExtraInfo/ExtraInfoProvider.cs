using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Scopes.ExtraInfo;

public class ExtraInfoProvider(MembersStorage membersStorage) : IInfoProvider
{
    public async Task<Dictionary<string, string>> GetInfo(Guid id)
    {
        var info = await membersStorage.GetExtraInfo(id);

        return new Dictionary<string, string>
        {
            { nameof(ExtraMemberInfo.Description), info.Description }
        };
    }
}