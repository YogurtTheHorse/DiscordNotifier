using Telegram.Bot;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class RegisterChat(ITelegramBotClient telegramBotClient, ChatsRegistry chatsRegistry)
    : ICommandListener
{
    public string Command => "registerChat";

    public string Description => "saves current chat as managed";

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public string[] RequiredPermissions { get; } =
    [
        "chats.edit"
    ];

    public async Task Execute(CommandContext commandContext)
    {
        // todo: fix telegram-only code
        var chat = long.Parse(commandContext.ChatId);

        var admins = await telegramBotClient.GetChatAdministratorsAsync(chat);

        if (admins.All(a => a.User.Id != telegramBotClient.BotId))
        {
            await commandContext.Reply("Bot is not admin in this chat");

            return;
        }

        var registered = await chatsRegistry.RegisterTelegramChat(chat);

        await commandContext.Reply(registered ? "Ok" : "Already registered");
    }
}