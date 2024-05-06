using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace YogurtTheCommunity.DiscordNotifier.Services;

public class EventManager
{
    private readonly IOptions<DiscordNotifierOptions> _notifierOptions;
    private readonly ChannelsStateManager _channelsStateManager;
    private readonly TimingsManager _timingsManager;
    private readonly ITelegramBotClient _telegramBotClient;

    private long ChatId => _notifierOptions.Value.TelegramTargetId;
    private int? ThreadId => _notifierOptions.Value.TelegramThreadId;

    public EventManager(
        IOptions<DiscordNotifierOptions> notifierOptions,
        ChannelsStateManager channelsStateManager,
        TimingsManager timingsManager,
        ITelegramBotClient telegramBotClient
    )
    {
        _notifierOptions = notifierOptions;
        _channelsStateManager = channelsStateManager;
        _timingsManager = timingsManager;
        _telegramBotClient = telegramBotClient;
    }

    public async Task LeftChannel(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);
    }

    public async Task JoinedChannel(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);

        var lastJoined = await _timingsManager.GetLastReportedJoinTime(user.Id);
    }

    public async Task UserSwitchedChannel(SocketUser _, SocketVoiceChannel beforeChannel, SocketVoiceChannel afterChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(beforeChannel);
        await _channelsStateManager.UpdateChannelInfo(afterChannel);
    }

    public async Task UserStoppedStreaming(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);
        await _timingsManager.SetLastStreamingTime(user.Id, DateTime.Now);
    }

    public async Task UserStartedStreaming(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);

        var lastJoined = await _timingsManager.GetLastStreamingTime(user.Id);

        if ((DateTime.Now - lastJoined).TotalSeconds > _notifierOptions.Value.WaitBetweenStreaming)
        {
            await _timingsManager.SetLastStreamingTime(user.Id, DateTime.Now);

            await _telegramBotClient.SendTextMessageAsync(
                ChatId,
                $"<b>{user.Username}</b> started streaming inside <b>{voiceChannel.Name}</b>",
                parseMode: ParseMode.Html,
                messageThreadId: ThreadId
            );
        }
    }
}