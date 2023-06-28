using Discord.WebSocket;
using DiscordNotifier;
using DiscordNotifier.Options;
using Microsoft.Extensions.Options;
using Telegram.Bot;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<DiscordWorker>();

        services.Configure<TelegramOptions>(hostContext.Configuration.GetSection("Telegram"));
        services.Configure<DiscordOptions>(hostContext.Configuration.GetSection("Discord"));

        services.AddSingleton<ITelegramBotClient>(s =>
        {
            var o = s.GetRequiredService<IOptions<TelegramOptions>>();

            return new TelegramBotClient(o.Value.Token);
        });
        services.AddSingleton<DiscordSocketClient>(_ => new DiscordSocketClient());
    })
    .Build();

host.Run();