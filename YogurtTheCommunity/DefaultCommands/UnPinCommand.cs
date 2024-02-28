using Telegram.Bot;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class UnPinCommand : ICommandListener
{
    private readonly ITelegramBotClient _telegramBotClient;

    public string Command => "unpin";

    public string Description => "unpins message";

    public string[] RequiredPermissions { get; } = {
        "messages.unpin"
    };

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public UnPinCommand(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public async Task Execute(CommandContext commandContext)
    {
        if (commandContext.ReplyToMessageId is null)
        {
            await commandContext.Reply("Reply to some message to unpin it");
            return;
        }
        
        var chat = long.Parse(commandContext.ChatId);
        var messageId = int.Parse(commandContext.ReplyToMessageId);

        await _telegramBotClient.UnpinChatMessageAsync(chat, messageId);
    }
}