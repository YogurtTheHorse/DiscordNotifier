using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Scopes.ExtraInfo;

public class ExtraInfoProvider : IInfoProvider
{
    private readonly MembersStorage _membersStorage;

    public ExtraInfoProvider(MembersStorage membersStorage)
    {
        _membersStorage = membersStorage;
    }

    public async Task<Dictionary<string, string>> GetInfo(Guid id)
    {
        var info = await _membersStorage.GetExtraInfo(id);

        return new Dictionary<string, string>
        {
            { nameof(ExtraMemberInfo.Description), info.Description }
        };
    }
}