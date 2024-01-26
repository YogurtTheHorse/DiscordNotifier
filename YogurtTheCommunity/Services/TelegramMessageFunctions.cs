using Scriban.Runtime;

namespace YogurtTheCommunity.Services;

public class TelegramMessageFunctions : ScriptObject
{
    private readonly MembersStorage _membersStorage;

    public TelegramMessageFunctions(MembersStorage membersStorage)
    {
        _membersStorage = membersStorage;

        SetValue("bold", new DelegateCustomFunction(Bold), true);
        SetValue("mention", new DelegateCustomFunction(Mention), true);
    }
    
    public string Bold(string text) => $"<b>{text}</b>";
    
    public async Task<string> Mention(Guid guid, string name)
    {
        var tgId = await _membersStorage.GetTelegramId(guid);

        return tgId.HasValue
            ? $"<a href=\"tg://user?id={tgId.Value}\">{name}</a>"
            : name;
    }
}