using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.TitleGiver;

public class TitleJoinListener(TitlesStorage titlesStorage, MembersStorage membersStorage, TitlesManager titlesManager)
    : ITelegramUpdateListener
{
    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChatMember: { Chat: { } chat, NewChatMember: { User: { } newChatMember , Status: ChatMemberStatus.Member} }}) return;
        if (chat.Type == ChatType.Channel) return;

        var member = await membersStorage.GetMemberByTelegramId(newChatMember.Id);
        
        if (member is null) return;

        var title = await titlesStorage.GetTitle(member.Id);
        
        if (title is null) return;

        await titlesManager.UpdateTitleInChat(member.Id, title, chat.Id, newChatMember.Id);
        await client.SendTextMessageAsync(chat, "Automatically setting title...", cancellationToken: cts);
    }
}