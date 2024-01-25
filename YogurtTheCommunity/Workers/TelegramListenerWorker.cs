using Telegram.Bot;
using Telegram.Bot.Types;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Workers;

public class TelegramListenerWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;

    private readonly MembersStorage _membersStorage;

    private readonly ICommandListener[] _commandListeners;
    private readonly IEnumerable<ITelegramUpdateListener> _updateListeners;

    private readonly ILogger<TelegramListenerWorker> _logger;

    public TelegramListenerWorker(
        ITelegramBotClient botClient,
        IEnumerable<ICommandListener> commandListeners,
        IEnumerable<ITelegramUpdateListener> updateListeners,
        MembersStorage membersStorage,
        ILogger<TelegramListenerWorker> logger
    )
    {
        _botClient = botClient;
        _updateListeners = updateListeners;
        _membersStorage = membersStorage;
        _logger = logger;
        _commandListeners = commandListeners.ToArray();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(OnUpdate, OnPollingError, cancellationToken: stoppingToken);

        return Task.CompletedTask;
    }

    private Task OnPollingError(ITelegramBotClient client, Exception exception, CancellationToken cts)
    {
        _logger.LogError(exception, "Telegram polling error");

        return Task.CompletedTask;
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts) =>
        await Task.WhenAll(
            _updateListeners
                .Select(x => x.OnUpdate(client, update, cts))
                .Append(InternalUpdateProcess(client, update, cts))
        );

    private async Task InternalUpdateProcess(ITelegramBotClient _, Update update, CancellationToken cts)
    {
        switch (update)
        {
            case { Message: { } message }:
                await ProcessMessage(message);

                break;
        }
    }

    private async Task ProcessMessage(Message message)
    {
        if (!TryParseCommandName(message.Text, out string command)) return;

        var commandListener = _commandListeners.FirstOrDefault(
            x => x.Command.Equals(command, StringComparison.InvariantCultureIgnoreCase)
        );

        if (commandListener is null) return;

        var commandContext = await GetCommandContext(message, commandListener);

        await commandListener.Execute(commandContext);
    }

    private async Task<CommandContext> GetCommandContext(Message message, ICommandListener commandListener)
    {
        var argsStart = message.Text?.IndexOf(' ') ?? -1;
        var argsString = argsStart == -1
            ? Array.Empty<string>()
            : message.Text![(argsStart + 1)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var arguments = new Dictionary<CommandArgument, string>();

        for (var i = 0; i < commandListener.Arguments.Count && i < argsString.Length; i++)
        {
            var argument = commandListener.Arguments[i];
            arguments.Add(argument, argsString[i]);
        }

        return new CommandContext(
            arguments,
            async text => await _botClient.SendTextMessageAsync(message.Chat.Id, text),
            await GetMemberInfo(message.From!),
            message.ReplyToMessage is { From: { } replyTo } ? await GetMemberInfo(replyTo) : null
        );
    }

    public async Task<MemberInfo> GetMemberInfo(User user)
    {
        var name = $"{user.FirstName} {user.LastName}";

        return await _membersStorage.GetMemberByTelegramId(user.Id)
               ?? await _membersStorage.CreateFromTelegram(user.Id, name);
    }

    private static bool TryParseCommandName(string? messageText, out string command)
    {
        if (messageText is null || !messageText.StartsWith("/"))
        {
            command = string.Empty;

            return false;
        }

        var commandEndIndex = messageText.IndexOf(' ', 1);

        command = commandEndIndex == -1
            ? messageText[1..]
            : messageText.Substring(1, commandEndIndex - 1);

        var atIndex = command.IndexOf('@');

        if (atIndex != -1)
        {
            command = command[..atIndex];
        }

        return true;
    }
}