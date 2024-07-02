using Telegram.Bot;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class PinCommand(ITelegramBotClient telegramBotClient) : ICommandListener
{
    public string Command => "pin";

    public string Description => "pins message";

    public string[] RequiredPermissions { get; } = {
        "messages.pin"
    };

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("notify", "false")
    };

    public async Task Execute(CommandContext commandContext)
    {
        if (commandContext.ReplyToMessageId is null)
        {
            await commandContext.Reply("Reply to some message to pin it");
            return;
        }
        
        var chat = long.Parse(commandContext.ChatId);
        var messageId = int.Parse(commandContext.ReplyToMessageId);
        var notify = commandContext.GetArgument(Arguments[0]).AsBool() ?? false;

        await telegramBotClient.PinChatMessageAsync(chat, messageId, notify);
    }
}