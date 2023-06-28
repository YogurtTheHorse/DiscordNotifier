using Discord;
using Discord.WebSocket;
using DiscordNotifier.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace DiscordNotifier;

public class DiscordWorker : BackgroundService
{
    private readonly ILogger<DiscordWorker> _logger;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IOptions<TelegramOptions> _telegramOptions;
    private readonly IOptions<DiscordOptions> _discordOptions;

    public DiscordWorker(
        ILogger<DiscordWorker> logger,
        ITelegramBotClient telegramBotClient,
        DiscordSocketClient discordSocketClient,
        IOptions<TelegramOptions> telegramOptions,
        IOptions<DiscordOptions> discordOptions)
    {
        _logger = logger;
        _telegramBotClient = telegramBotClient;
        _discordSocketClient = discordSocketClient;
        _telegramOptions = telegramOptions;
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
        if (afterState.VoiceChannel is null) return;

        await _telegramBotClient.SendTextMessageAsync(
            _telegramOptions.Value.TargetId,
            $"{user.Username} joined {afterState.VoiceChannel.Name}"
        );
    }
}