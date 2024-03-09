using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.TitleGiver;

public class TitleJoinListener : ITelegramUpdateListener
{
    private readonly TitlesStorage _titlesStorage;
    private readonly MembersStorage _membersStorage;
    private readonly TitlesManager _titlesManager;

    public TitleJoinListener(TitlesStorage titlesStorage, MembersStorage membersStorage, TitlesManager titlesManager)
    {
        _titlesStorage = titlesStorage;
        _membersStorage = membersStorage;
        _titlesManager = titlesManager;
    }

    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChatMember: { Chat: { } chat, NewChatMember: { User: { } newChatMember , Status: ChatMemberStatus.Member} }}) return;
        if (chat.Type == ChatType.Channel) return;

        var member = await _membersStorage.GetMemberByTelegramId(newChatMember.Id);
        
        if (member is null) return;

        var title = await _titlesStorage.GetTitle(member.Id);
        
        if (title is null) return;

        await _titlesManager.UpdateTitleInChat(member.Id, title, chat.Id, newChatMember.Id);
        await client.SendTextMessageAsync(chat, "Automatically setting title...", cancellationToken: cts);
    }
}