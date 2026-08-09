using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class ChannelsStateManager(
    ITelegramBotClient botClient,
    IOptions<DiscordNotifierOptions> notifierOptions,
    IBackgroundJobClient jobs,
    MessagesDataStorage messagesDataStorage)
{
    private long ChatId => notifierOptions.Value.TelegramTargetId;

    private int? ThreadId => notifierOptions.Value.TelegramThreadId;

    private bool NeedToPinMessage => notifierOptions.Value.NeedToPinMessage;

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

        var messageId = await messagesDataStorage.GetChannelStateMessage(channelId);

        if (messageId.HasValue)
        {
            try
            {
                await botClient.EditMessageText(ChatId, messageId.Value, stateMessage, parseMode: ParseMode.Html);
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
            var jobId = jobs.Schedule(
                (ChannelsStateManager m) => m.DeleteChannelMessage(channelId),
                TimeSpan.FromSeconds(notifierOptions.Value.WaitBeforeStatusDelete)
            );

            await messagesDataStorage.SaveDeleteMessageJobId(channelId, jobId);
        }

        async Task SendMessage()
        {
            var message = await botClient.SendMessage(ChatId, stateMessage, parseMode: ParseMode.Html,
                messageThreadId: ThreadId);

            await messagesDataStorage.SetChannelStateMessage(channelId, message.MessageId);
            if (NeedToPinMessage)
            {
                await botClient.PinChatMessage(ChatId, message.MessageId);
            }
        }
    }

    public async Task DeleteChannelMessage(ulong channelId)
    {
        var messageId = await messagesDataStorage.GetChannelStateMessage(channelId);

        if (!messageId.HasValue) return;

        try
        {
            await botClient.DeleteMessage(ChatId, messageId.Value);
            await messagesDataStorage.SetChannelStateMessage(channelId, null);
        }
        catch
        {
            // ignored
        }
    }

    public async Task CancelChannelMessageDelete(ulong channelId)
    {
        var jobId = await messagesDataStorage.GetDeleteMessageJobId(channelId);

        if (jobId is null) return;

        jobs.Delete(jobId);
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