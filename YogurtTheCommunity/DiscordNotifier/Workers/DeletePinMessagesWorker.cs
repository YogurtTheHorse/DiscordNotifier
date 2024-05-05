using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.DiscordNotifier.Workers;

public class DeletePinMessagesWorker : ITelegramUpdateListener
{
    private readonly IOptions<DiscordNotifierOptions> _notifierOptions;

    public DeletePinMessagesWorker(IOptions<DiscordNotifierOptions> notifierOptions)
    {
        _notifierOptions = notifierOptions;
    }

    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChannelPost : { Chat.Id: var chatId, PinnedMessage: not null, MessageId: { } messageId } }
            || chatId != _notifierOptions.Value.TelegramTargetId) return;

        try
        {
            await client.DeleteMessageAsync(chatId, messageId, cts);
        }
        catch
        {
            // ignored
        }
    }
}