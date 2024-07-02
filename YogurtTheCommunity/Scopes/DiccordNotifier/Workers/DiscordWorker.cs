using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using YogurtTheCommunity.DiscordNotifier.Services;

namespace YogurtTheCommunity.DiscordNotifier.Workers;

public class DiscordWorker(
    ILogger<DiscordWorker> logger,
    EventManager eventManager,
    DiscordSocketClient discordSocketClient,
    ChannelsStateManager channelsStateManager,
    IOptions<DiscordNotifierOptions> notifierOptions)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!notifierOptions.Value.Enabled)
        {
            logger.LogInformation("Not enabling DiscordWorker because it's disabled in config");
            return;
        }
        
        discordSocketClient.Log += Log;
        discordSocketClient.VoiceChannelStatusUpdated += OnUserVoiceStateUpdated;
        discordSocketClient.UserVoiceStateUpdated += OnUserVoiceStateUpdated;

        await discordSocketClient.LoginAsync(TokenType.Bot, notifierOptions.Value.Token);
        await discordSocketClient.StartAsync();

        await Task.Delay(-1, stoppingToken);

        await discordSocketClient.StopAsync();
        discordSocketClient.UserVoiceStateUpdated -= OnUserVoiceStateUpdated;
        discordSocketClient.VoiceChannelStatusUpdated -= OnUserVoiceStateUpdated;
        discordSocketClient.Log -= Log;
    }

    private Task Log(LogMessage msg)
    {
        logger.LogInformation("{msg}", msg);

        return Task.CompletedTask;
    }

    private async Task OnUserVoiceStateUpdated(Cacheable<SocketVoiceChannel, ulong> cacheable, string oldStatus, string newStatus)
    {
        var voiceChannel = await cacheable.GetOrDownloadAsync();
        
        if (voiceChannel == null) return;

        await channelsStateManager.UpdateChannelInfo(voiceChannel);
    }

    private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
    {
        switch (new {
                    Before = beforeState,
                    After = afterState
                })
        {
            case { Before.VoiceChannel: null, After.VoiceChannel: { } vc }:
                await eventManager.JoinedChannel(user, vc);

                break;

            case { Before.VoiceChannel: { } vc, After.VoiceChannel: null }:
                await eventManager.LeftChannel(user, vc);

                break;

            case { Before.VoiceChannel: { } beforeChannel, After.VoiceChannel: { } afterChannel }
                when beforeChannel.Id == afterChannel.Id:
                await OnStateInsideChannelUpdated(user, beforeState, afterState);

                break;

            case { Before.VoiceChannel: { } beforeChannel, After.VoiceChannel: { } afterChannel }
                when beforeChannel.Id != afterChannel.Id:
                await eventManager.UserSwitchedChannel(user, beforeChannel, afterChannel);

                break;
        }
    }

    private async Task OnStateInsideChannelUpdated(SocketUser user, SocketVoiceState beforeState, SocketVoiceState afterState)
    {
        if (beforeState.IsStreaming && !afterState.IsStreaming)
        {
            await eventManager.UserStoppedStreaming(user, afterState.VoiceChannel);
        }
        else if (!beforeState.IsStreaming && afterState.IsStreaming)
        {
            await eventManager.UserStartedStreaming(user, afterState.VoiceChannel);
        }
        else // just update state
        {
            await channelsStateManager.UpdateChannelInfo(afterState.VoiceChannel);
        }
    }
}