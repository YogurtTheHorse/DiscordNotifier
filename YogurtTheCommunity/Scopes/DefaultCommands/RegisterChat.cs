using Telegram.Bot;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class RegisterChat : ICommandListener
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ChatsRegistry _chatsRegistry;

    public string Command => "registerChat";

    public string Description => "saves current chat as managed";

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public string[] RequiredPermissions { get; } = {
        "chats.edit"
    };

    public RegisterChat(ITelegramBotClient telegramBotClient, ChatsRegistry chatsRegistry)
    {
        _telegramBotClient = telegramBotClient;
        _chatsRegistry = chatsRegistry;
    }

    public async Task Execute(CommandContext commandContext)
    {
        // todo: fix telegram-only code
        var chat = long.Parse(commandContext.ChatId);

        var admins = await _telegramBotClient.GetChatAdministratorsAsync(chat);

        if (admins.All(a => a.User.Id != _telegramBotClient.BotId))
        {
            await commandContext.Reply("Bot is not admin in this chat");

            return;
        }

        var registered = await _chatsRegistry.RegisterTelegramChat(chat);

        await commandContext.Reply(registered ? "Ok" : "Already registered");
    }
}