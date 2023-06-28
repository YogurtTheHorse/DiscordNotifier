using Discord.WebSocket;
using DiscordNotifier.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiscordNotifier.Services;

public class EventManager
{
    private readonly IOptions<TelegramOptions> _telegramOptions;
    private readonly IOptions<WaitOptions> _waitOptions;
    private readonly ChannelsStateManager _channelsStateManager;
    private readonly TimingsManager _timingsManager;
    private readonly ITelegramBotClient _telegramBotClient;

    private long ChatId => _telegramOptions.Value.TargetId;

    public EventManager(
        IOptions<TelegramOptions> telegramOptions,
        IOptions<WaitOptions> waitOptions,
        ChannelsStateManager channelsStateManager,
        TimingsManager timingsManager,
        ITelegramBotClient telegramBotClient)
    {
        _telegramOptions = telegramOptions;
        _waitOptions = waitOptions;
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

        if ((DateTime.Now - lastJoined).TotalSeconds > _waitOptions.Value.WaitBetweenJoins)
        {
            await _timingsManager.SetLastReportedJoinTime(user.Id, DateTime.Now);

            await _telegramBotClient.SendTextMessageAsync(
                ChatId,
                $"<b>{user.Username}</b> joined <b>{voiceChannel.Name}</b>",
                parseMode: ParseMode.Html
            );
        }
    }

    public async Task UserSwitchedChannel(SocketUser _, SocketVoiceChannel beforeChannel, SocketVoiceChannel afterChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(beforeChannel);
        await _channelsStateManager.UpdateChannelInfo(afterChannel);
    }

    public async Task UserStoppedStreaming(SocketUser _, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);
    }

    public async Task UserStartedStreaming(SocketUser user, SocketVoiceChannel voiceChannel)
    {
        await _channelsStateManager.UpdateChannelInfo(voiceChannel);

        var lastJoined = await _timingsManager.GetLastStreamingTime(user.Id);

        if ((DateTime.Now - lastJoined).TotalSeconds > _waitOptions.Value.WaitBetweenStreaming)
        {
            await _timingsManager.SetLastStreamingTime(user.Id, DateTime.Now);

            await _telegramBotClient.SendTextMessageAsync(
                ChatId,
                $"<b>{user.Username}</b> started streaming inside <b>{voiceChannel.Name}</b>",
                parseMode: ParseMode.Html
            );
        }
    }
}