using Telegram.Bot;
using Telegram.Bot.Types;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.JoinsManager;

public class JoinListener : ITelegramUpdateListener
{
    private readonly JoinsStorage _joinsStorage;
    private readonly MembersStorage _membersStorage;

    public JoinListener(JoinsStorage joinsStorage, MembersStorage membersStorage)
    {
        _joinsStorage = joinsStorage;
        _membersStorage = membersStorage;
    }
    
    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChatJoinRequest: { Chat: { } chat, From: { } joiner } }) return;
        
        var member = await _membersStorage.GetOrCreate(joiner);
        var isApproved = await _joinsStorage.IsApproved(member.Id);

        if (isApproved)
        {
            await client.ApproveChatJoinRequest(chat.Id, joiner.Id, cts);
            return;
        }

        await _joinsStorage.AddJoinRequest(member.Id, chat.Id);
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