using Discord;
using Discord.WebSocket;
using DiscordNotifier.Options;
using DiscordNotifier.Services;
using Microsoft.Extensions.Options;

namespace DiscordNotifier.Workers;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly EventManager _eventManager;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly ChannelsStateManager _channelsStateManager;
    private readonly IOptions<DiscordOptions> _discordOptions;

    public DiscordWorker(
        ILogger<DiscordWorker> logger,
        EventManager eventManager,
        DiscordSocketClient discordSocketClient,
        ChannelsStateManager channelsStateManager,
        IOptions<DiscordOptions> discordOptions)
    {
        _logger = logger;
        _eventManager = eventManager;
        _discordSocketClient = discordSocketClient;
        _channelsStateManager = channelsStateManager;
        _discordOptions = discordOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _discordSocketClient.Log += Log;
        _discordSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;

        await _discordSocketClient.LoginAsync(TokenType.Bot, _discordOptions.Value.Token);
        await _discordSocketClient.StartAsync();

        await Task.Delay(-1, stoppingToken);

        await _discordSocketClient.StopAsync();
        _discordSocketClient.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
        _discordSocketClient.Log -= Log;
    }

    private Task Log(LogMessage msg)
    {
        _logger.LogInformation("{msg}", msg);

        return Task.CompletedTask;
    }

    private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
    {
        switch (new {
                    Before = beforeState,
                    After = afterState
                })
        {
            case { Before.VoiceChannel: null, After.VoiceChannel: { } vc }:
                await _eventManager.JoinedChannel(user, vc);

                break;

            case { Before.VoiceChannel: { } vc, After.VoiceChannel: null }:
                await _eventManager.LeftChannel(user, vc);

                break;

            case { Before.VoiceChannel: { } beforeChannel, After.VoiceChannel: { } afterChannel }
                when beforeChannel.Id == afterChannel.Id:
                await OnStateInsideChannelUpdated(user, beforeState, afterState);

                break;

            case { Before.VoiceChannel: { } beforeChannel, After.VoiceChannel: { } afterChannel }
                when beforeChannel.Id != afterChannel.Id:
                await _eventManager.UserSwitchedChannel(user, beforeChannel, afterChannel);

                break;
        }
    }

    private async Task OnStateInsideChannelUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
    {
        if (beforeState.IsStreaming && !afterState.IsStreaming)
        {
            await _eventManager.UserStoppedStreaming(user, afterState.VoiceChannel);
        }
        else if (!beforeState.IsStreaming && afterState.IsStreaming)
        {
            await _eventManager.UserStartedStreaming(user, afterState.VoiceChannel);
        }
        else // just update state
        {
            await _channelsStateManager.UpdateChannelInfo(afterState.VoiceChannel);
        }
    }
}