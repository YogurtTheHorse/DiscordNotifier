using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.DiscordNotifier.Workers;

public class DeletePinMessagesWorker(IOptions<DiscordNotifierOptions> notifierOptions) : ITelegramUpdateListener
{
    public async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        if (update is not { ChannelPost : { Chat.Id: var chatId, PinnedMessage: not null, MessageId: var messageId } }
            || chatId != notifierOptions.Value.TelegramTargetId) return;

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