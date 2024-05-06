using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class ChannelsStateManager
{
    private readonly ITelegramBotClient _botClient;
    private readonly IOptions<DiscordNotifierOptions> _notifierOptions;
    private readonly IBackgroundJobClient _jobs;
    private readonly MessagesDataStorage _messagesDataStorage;

    private long ChatId => _notifierOptions.Value.TelegramTargetId;

    private int? ThreadId => _notifierOptions.Value.TelegramThreadId;

    private bool NeedToPinMessage => _notifierOptions.Value.NeedToPinMessage;

    public ChannelsStateManager(
        ITelegramBotClient botClient,
        IOptions<DiscordNotifierOptions> notifierOptions,
        IBackgroundJobClient jobs,
        MessagesDataStorage messagesDataStorage)
    {
        _botClient = botClient;
        _notifierOptions = notifierOptions;
        _jobs = jobs;
        _messagesDataStorage = messagesDataStorage;
    }

    public async Task UpdateChannelInfo(SocketVoiceChannel voiceChannel)
    {
        var channelId = voiceChannel.Id;
        var tags = string.Empty;
        var hasUsers = voiceChannel.ConnectedUsers.Count > 0;

        if (voiceChannel.UserLimit.HasValue)
        {
            tags += $"{voiceChannel.ConnectedUsers.Count}/{voiceChannel.UserLimit}";
        }

        var users = hasUsers
            ? string.Join("\n", voiceChannel.ConnectedUsers.Select(UserToText).ToArray())
            : "Nobody here anymore...";

        var channelStatus = voiceChannel.Status is null ? string.Empty : "\n" + voiceChannel.Status;
        var stateMessage = $"<b>{voiceChannel.Name}</b> {tags}{channelStatus}\n\n{users}";

        var messageId = await _messagesDataStorage.GetChannelStateMessage(channelId);

        if (messageId.HasValue)
        {
            try
            {
                await _botClient.EditMessageTextAsync(ChatId, messageId.Value, stateMessage, parseMode: ParseMode.Html);
            }
            catch (ApiRequestException ex) // we weren't able to edit, so send another one
            {
                if (ex.Message.Contains("not modified")) return;

                await SendMessage();
            }
        }
        else
        {
            await SendMessage();
        }

        if (hasUsers)
        {
            await CancelChannelMessageDelete(channelId);
        }
        else
        {
            var jobId = _jobs.Schedule(
                (ChannelsStateManager m) => m.DeleteChannelMessage(channelId),
                TimeSpan.FromSeconds(_notifierOptions.Value.WaitBeforeStatusDelete)
            );

            await _messagesDataStorage.SaveDeleteMessageJobId(channelId, jobId);
        }

        async Task SendMessage()
        {
            var message = await _botClient.SendTextMessageAsync(ChatId, stateMessage, parseMode: ParseMode.Html, messageThreadId: ThreadId);

            await _messagesDataStorage.SetChannelStateMessage(channelId, message.MessageId);

            if (NeedToPinMessage)
            {
                await _botClient.PinChatMessageAsync(ChatId, message.MessageId);
            }
        }
    }

    public async Task DeleteChannelMessage(ulong channelId)
    {
        var messageId = await _messagesDataStorage.GetChannelStateMessage(channelId);

        if (!messageId.HasValue) return;

        try
        {
            await _botClient.DeleteMessageAsync(ChatId, messageId.Value);
            await _messagesDataStorage.SetChannelStateMessage(channelId, null);
        }
        catch
        {
            // ignored
        }
    }

    public async Task CancelChannelMessageDelete(ulong channelId)
    {
        var jobId = await _messagesDataStorage.GetDeleteMessageJobId(channelId);

        if (jobId is null) return;

        _jobs.Delete(jobId);
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

    private string GetDeleteMessageJobName(SocketVoiceChannel channel) => $"delete-message-{channel.Id}";
}