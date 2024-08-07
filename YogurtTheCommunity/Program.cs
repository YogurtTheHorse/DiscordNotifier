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
using YogurtTheCommunity.JoinsManager;
using YogurtTheCommunity.Scopes.Exceptions;
using YogurtTheCommunity.Scopes.ExtraInfo;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Subscriptions;
using YogurtTheCommunity.TitleGiver;
using YogurtTheCommunity.Workers;

var builder = Host.CreateApplicationBuilder(args);
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? string.Empty);


#region Default

builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<MembersDefaultOptions>(builder.Configuration.GetSection("Members"));
builder.Services.Configure<PermissionsOptions>(builder.Configuration.GetSection("Permissions"));

builder.Services.AddHostedService<TelegramListenerWorker>();
builder.Services.AddSingleton<ICommandListener, InfoCommand>();
builder.Services.AddSingleton<ICommandListener, SetNameCommand>();
builder.Services.AddSingleton<ICommandListener, AddRoleCommand>();
builder.Services.AddSingleton<ICommandListener, RegisterChat>();
builder.Services.AddSingleton<ICommandListener, RemoveRoleCommand>();
builder.Services.AddSingleton<ICommandListener, PinCommand>();
builder.Services.AddSingleton<ICommandListener, UnPinCommand>();

builder.Services.AddSingleton<MembersStorage>();
builder.Services.AddSingleton<ChatsRegistry>();
builder.Services.AddSingleton<PermissionsManager>();
builder.Services.AddSingleton<CommandExecutor>();

#endregion

#region Extra info

builder.Services.AddSingleton<ICommandListener, SetDescriptionCommand>();
builder.Services.AddSingleton<IInfoProvider, ExtraInfoProvider>();

#endregion

#region Exceptions

builder.Services.AddSingleton<ICommandListener, AddExceptionCommand>();
builder.Services.AddSingleton<ICommandListener, RemoveException>();
builder.Services.AddSingleton<ICommandListener, ListExceptionsCommand>();
builder.Services.AddSingleton<ExceptionsStorage>();

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

#region Titles

builder.Services.AddSingleton<ITelegramUpdateListener, TitleJoinListener>();
builder.Services.AddSingleton<ICommandListener, SetTitleCommand>();
builder.Services.AddSingleton<ICommandListener, SyncTitleCommand>();
builder.Services.AddSingleton<IInfoProvider, TitleInfoProvider>();
builder.Services.AddSingleton<TitlesManager>();
builder.Services.AddSingleton<TitlesStorage>();

#endregion

#region Joins

builder.Services.AddSingleton<ITelegramUpdateListener, JoinListener>();
builder.Services.AddSingleton<ICommandListener, ApproveCommand>();
builder.Services.AddSingleton<JoinsStorage>();

#endregion

#region Notify

builder.Services.AddSingleton<ICommandListener, NotifyCommand>();
builder.Services.AddSingleton<ICommandListener, SubscribeCommand>();
builder.Services.AddSingleton<ICommandListener, UnSubscribeCommand>();
builder.Services.AddSingleton<SubscriptionsStorage>();
builder.Services.AddSingleton<IInfoProvider, UserSubscriptionsInfo>();

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