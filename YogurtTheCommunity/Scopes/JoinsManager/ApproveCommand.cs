using Telegram.Bot;
using Telegram.Bot.Exceptions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.JoinsManager;

public class ApproveCommand : ICommandListener
{
    private readonly JoinsStorage _joinsStorage;
    private readonly MembersStorage _membersStorage;
    private readonly ITelegramBotClient _botClient;

    public string Command => "approveJoin";

    public string Description => "approves join request";

    public string[] RequiredPermissions { get; } = {
        "joins.manager"
    };

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("memberId")
    };

    public ApproveCommand(JoinsStorage joinsStorage, MembersStorage membersStorage, ITelegramBotClient botClient)
    {
        _joinsStorage = joinsStorage;
        _membersStorage = membersStorage;
        _botClient = botClient;
    }

    public async Task Execute(CommandContext commandContext)
    {
        if (!Guid.TryParse(commandContext.GetArgument(Arguments[0]), out var memberId))
        {
            await commandContext.Reply("Invalid member id");

            return;
        }
        
        var telegramId = await _membersStorage.GetTelegramId(memberId);
        
        if (telegramId is null)
        {
            await commandContext.Reply("Member not found");

            return;
        }

        await _joinsStorage.Approve(memberId);
        var joinRequestsChats = await _joinsStorage.GetJoinRequests(memberId, true);

        foreach (var chatId in joinRequestsChats)
        {
            try
            {
                await _botClient.ApproveChatJoinRequest(chatId, telegramId.Value);
            }
            catch (ApiRequestException ex)
            {
                await commandContext.Reply($"Error approving join request in chat {chatId}: {ex.Message}"); 
            }
        }

        await commandContext.Reply("Done");
    }
}