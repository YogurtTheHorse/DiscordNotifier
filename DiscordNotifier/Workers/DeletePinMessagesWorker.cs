using DiscordNotifier.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiscordNotifier.Workers;

public class DeletePinMessagesWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<TelegramOptions> _telegramOptions;

    public DeletePinMessagesWorker(ITelegramBotClient botClient, IOptions<TelegramOptions> telegramOptions)
    {
        _botClient = botClient;
        _telegramOptions = telegramOptions;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(OnUpdate, OnPollingError, cancellationToken: stoppingToken);

        return Task.CompletedTask;
    }

    private Task OnPollingError(ITelegramBotClient _, Exception e, CancellationToken cts) => Task.CompletedTask;

    private async Task OnUpdate(ITelegramBotClient _, Update update, CancellationToken cts)
    {
        if (update is not { ChannelPost : { Chat.Id: { } chatId, PinnedMessage: not null, MessageId: { } messageId } }
            || chatId != _telegramOptions.Value.TargetId) return;

        try
        {
            await _botClient.DeleteMessageAsync(chatId, messageId, cts);
        }
        catch
        {
            // ignored
        }
    }
}