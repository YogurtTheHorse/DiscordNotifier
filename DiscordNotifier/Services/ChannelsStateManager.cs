using Discord.WebSocket;
using DiscordNotifier.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiscordNotifier.Services;

public class ChannelsStateManager
{
    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<TelegramOptions> _telegramOptions;
    private readonly MessagesDataStorage _messagesDataStorage;

    private long ChatId => _telegramOptions.Value.TargetId;

    public ChannelsStateManager(
        ITelegramBotClient botClient,
        IOptions<TelegramOptions> telegramOptions,
        MessagesDataStorage messagesDataStorage)
    {
        _botClient = botClient;
        _telegramOptions = telegramOptions;
        _messagesDataStorage = messagesDataStorage;
    }

    public async Task UpdateChannelInfo(SocketVoiceChannel voiceChannel)
    {
        var tags = string.Empty;

        if (voiceChannel.UserLimit.HasValue)
        {
            tags += $"{voiceChannel.ConnectedUsers.Count}/{voiceChannel.UserLimit}";
        }

        var users = string.Join("\n", voiceChannel.ConnectedUsers.Select(UserToText).ToArray());
        var stateMessage = $"<b>{voiceChannel.Name}</b> {tags}\n\n{users}";

        var messageId = await _messagesDataStorage.GetChannelStateMessage(voiceChannel.Id);

        if (messageId.HasValue)
        {
            try
            {
                await _botClient.EditMessageTextAsync(ChatId, messageId.Value, stateMessage, parseMode: ParseMode.Html);
            }
            catch // we weren't able to edit, so send another one
            {
                await SendMessage();
            }
        }
        else
        {
            await SendMessage();
        }

        async Task SendMessage()
        {
            var message = await _botClient.SendTextMessageAsync(ChatId, stateMessage, parseMode: ParseMode.Html);

            await _messagesDataStorage.SetChannelStateMessage(voiceChannel.Id, message.MessageId);
            await _botClient.PinChatMessageAsync(ChatId, message.MessageId);
        }
    }

    private string UserToText(SocketGuildUser user)
    {
        var name = user.DisplayName;
        var tags = string.Empty;

        if (user.IsStreaming)
        {
            tags += "📺";
        }

        if (user.IsMuted || user.IsSelfMuted)
        {
            tags += "🙊";
        }

        if (user.IsDeafened || user.IsSelfDeafened)
        {
            tags += "🙉";
        }

        if (user.IsBot)
        {
            tags += "🤖";
        }

        if (user.IsVideoing)
        {
            tags += "📹";
        }

        var activities = string.Join(", ", user.Activities.Select(a => a.Name));

        var res = $"{name} {tags}";

        if (activities.Length > 0)
        {
            res += $" ({activities})";
        }

        return res;
    }
}