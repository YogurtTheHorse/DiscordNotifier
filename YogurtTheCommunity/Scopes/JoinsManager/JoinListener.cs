using Telegram.Bot;
using Telegram.Bot.Types;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.JoinsManager;

public class JoinListener(JoinsStorage joinsStorage, MembersStorage membersStorage) : ITelegramUpdateListener
{
    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChatJoinRequest: { Chat: { } chat, From: { } joiner } }) return;
        
        var member = await membersStorage.GetOrCreate(joiner);
        var isApproved = await joinsStorage.IsApproved(member.Id);

        if (isApproved)
        {
            await client.ApproveChatJoinRequest(chat.Id, joiner.Id, cts);
            return;
        }

        await joinsStorage.AddJoinRequest(member.Id, chat.Id);
        await client.SendTextMessageAsync(
            chat.Id,
            $"""
            A-hoy! Someone is trying to join chat!
            
            Name: {joiner.FirstName} {joiner.LastName}
            Username: @{joiner.Username}
            
            To approve this request, use /approveJoin {member.Id}
            """,
            cancellationToken: cts
        );
    }
}