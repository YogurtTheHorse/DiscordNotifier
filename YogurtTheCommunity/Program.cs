using Discord.WebSocket;
using Hangfire;
using YogurtTheCommunity.Options;
using YogurtTheCommunity.DiscordNotifier;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Telegram.Bot;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Commands.DefaultCommands;
using YogurtTheCommunity.DiscordNotifier.Services;
using YogurtTheCommunity.DiscordNotifier.Workers;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Workers;

var builder = Host.CreateApplicationBuilder(args);
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? string.Empty);


#region Default

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<MembersDefaultOptions>(builder.Configuration.GetSection("Members"));

builder.Services.AddHostedService<TelegramListenerWorker>();
builder.Services.AddSingleton<ICommandListener, InfoCommand>();
builder.Services.AddSingleton<ICommandListener, SetNameCommand>();

builder.Services.AddSingleton<MembersStorage>();

#endregion

#region DiscordNotifier

builder.Services.AddHostedService<DiscordWorker>();
builder.Services.AddSingleton<ITelegramUpdateListener, DeletePinMessagesWorker>();

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddSingleton<ChannelsStateManager>();
builder.Services.AddSingleton<EventManager>();
builder.Services.AddSingleton<TimingsManager>();

builder.Services.AddSingleton<MessagesDataStorage>();

builder.Services.Configure<DiscordNotifierOptions>(builder.Configuration.GetSection("DiscordNotifier"));

#endregion

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