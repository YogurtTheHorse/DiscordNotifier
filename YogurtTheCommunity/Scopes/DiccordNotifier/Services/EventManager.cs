using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class EventManager(
    IOptions<DiscordNotifierOptions> notifierOptions,
    ChannelsStateManager channelsStateManager,
    TimingsManager timingsManager,
    ITelegramBotClient telegramBotClient)
{
    private long ChatId => notifierOptions.Value.TelegramTargetId;

    public async Task LeftChannel(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await channelsStateManager.UpdateChannelInfo(voiceChannel);
    }

    public async Task JoinedChannel(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await channelsStateManager.UpdateChannelInfo(voiceChannel);

        var lastJoined = await timingsManager.GetLastReportedJoinTime(user.Id);
    }

    public async Task UserSwitchedChannel(SocketUser _, SocketVoiceChannel beforeChannel, SocketVoiceChannel afterChannel)
    {
        await channelsStateManager.UpdateChannelInfo(beforeChannel);
        await channelsStateManager.UpdateChannelInfo(afterChannel);
    }

    public async Task UserStoppedStreaming(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await channelsStateManager.UpdateChannelInfo(voiceChannel);
        await timingsManager.SetLastStreamingTime(user.Id, DateTime.Now);
    }

    public async Task UserStartedStreaming(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await channelsStateManager.UpdateChannelInfo(voiceChannel);

        var lastJoined = await timingsManager.GetLastStreamingTime(user.Id);

        if ((DateTime.Now - lastJoined).TotalSeconds > notifierOptions.Value.WaitBetweenStreaming)
        {
            await timingsManager.SetLastStreamingTime(user.Id, DateTime.Now);

            await telegramBotClient.SendTextMessageAsync(
                ChatId,
                $"<b>{user.Username}</b> started streaming inside <b>{voiceChannel.Name}</b>",
                parseMode: ParseMode.Html
            );
        }
    }
}