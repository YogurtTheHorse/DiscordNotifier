using Telegram.Bot;
using Telegram.Bot.Types;

namespace YogurtTheCommunity.Abstractions;

public interface ITelegramUpdateListener
{
    Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts);
}