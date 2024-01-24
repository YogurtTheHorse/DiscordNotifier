using Discord.WebSocket;
using Hangfire;
using YogurtTheCommunity.Options;
using YogurtTheCommunity.DiscordNotifier;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Telegram.Bot;
using YogurtTheCommunity.DiscordNotifier.Services;
using YogurtTheCommunity.DiscordNotifier.Workers;

var builder = Host.CreateApplicationBuilder(args);
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? string.Empty);

builder.Services.AddHostedService<DiscordWorker>();
builder.Services.AddHostedService<DeletePinMessagesWorker>();

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddSingleton<ChannelsStateManager>();
builder.Services.AddSingleton<EventManager>();
builder.Services.AddSingleton<TimingsManager>();

builder.Services.AddSingleton<MessagesDataStorage>();

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<DiscordNotifierOptions>(builder.Configuration.GetSection("DiscordNotifier"));

builder.Services.AddSingleton<ITelegramBotClient>(s =>
{
    var o = s.GetRequiredService<IOptions<TelegramOptions>>();

    return new TelegramBotClient(o.Value.Token);
});
builder.Services.AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient());

builder.Services.AddHangfire(configuration => configuration.UseRedisStorage(multiplexer,
    new RedisStorageOptions {
        Prefix = "community:hangfire:"
    }));
builder.Services.AddHangfireServer();

var host = builder.Build();
host.Run();