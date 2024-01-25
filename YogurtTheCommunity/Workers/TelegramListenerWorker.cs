using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Workers;

public class TelegramListenerWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;

    private readonly MembersStorage _membersStorage;
    private readonly PermissionsManager _permissionsManager;

    private readonly ICommandListener[] _commandListeners;
    private readonly IEnumerable<ITelegramUpdateListener> _updateListeners;

    private readonly ILogger<TelegramListenerWorker> _logger;

    public TelegramListenerWorker(
        ITelegramBotClient botClient,
        IEnumerable<ICommandListener> commandListeners,
        IEnumerable<ITelegramUpdateListener> updateListeners,
        MembersStorage membersStorage,
        PermissionsManager permissionsManager,
        ILogger<TelegramListenerWorker> logger
    )
    {
        _botClient = botClient;
        _updateListeners = updateListeners;
        _membersStorage = membersStorage;
        _permissionsManager = permissionsManager;
        _logger = logger;
        _commandListeners = commandListeners.ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(
            OnUpdate,
            OnPollingError,
            new ReceiverOptions {
                AllowedUpdates = new[] {
                    UpdateType.Message, UpdateType.ChatMember, UpdateType.CallbackQuery, UpdateType.ChannelPost,
                    UpdateType.ChatJoinRequest
                }
            },
            cancellationToken: stoppingToken
        );

        var commands = _commandListeners
            .Select(x => new BotCommand {
                Command = x.Command.ToLowerInvariant(),
                Description = $"{string.Join(' ', x.Arguments.Select(a => $"{a.Name}"))} - {x.Description}"
            })
            .ToArray();

        await _botClient.SetMyCommandsAsync(
            commands,
            cancellationToken: stoppingToken
        );
    }

    private Task OnPollingError(ITelegramBotClient client, Exception exception, CancellationToken cts)
    {
        _logger.LogError(exception, "Telegram polling error");

        return Task.CompletedTask;
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        await InternalUpdateProcess(update, cts);

        foreach (var listener in _updateListeners)
        {
            try
            {
                await listener.OnUpdate(client, update, cts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update listener {listener}", listener.GetType().Name);
            }
        }
    }

    private async Task InternalUpdateProcess(Update update, CancellationToken cts)
    {
        switch (update)
        {
            case { Message: { } message }:
                await ProcessMessage(message, cts);

                break;
        }
    }

    private async Task ProcessMessage(Message message, CancellationToken _)
    {
        if (!TryParseCommandName(message.Text, out var command)) return;

        var commandListener = _commandListeners.FirstOrDefault(
            x => x.Command.Equals(command, StringComparison.InvariantCultureIgnoreCase)
        );

        if (commandListener is null) return;

        var commandContext = await GetCommandContext(message, commandListener);

        // todo: create command executor

        if (!_permissionsManager.HasPermissions(commandContext.MemberInfo, commandListener.RequiredPermissions))
        {
            var roles = string.Join(", ", commandListener.RequiredPermissions);
            await commandContext.Reply($"You don't have permissions to execute this command ({roles})");

            return;
        }

        try
        {
            await commandListener.Execute(commandContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {command}", commandListener.Command);
            await commandContext.Reply($"Error executing command: {ex.Message}");
        }
    }

    private async Task<CommandContext> GetCommandContext(Message message, ICommandListener commandListener)
    {
        var argsStart = message.Text?.IndexOf(' ') ?? -1;
        var arguments = new Dictionary<CommandArgument, string>();

        if (argsStart > -1)
        {
            ParseArguments(message.Text!, commandListener, argsStart, arguments);
        }

        return new CommandContext(
            arguments,
            async text => await _botClient.SendTextMessageAsync(message.Chat.Id, text),
            await _membersStorage.GetOrCreate(message.From!),
            message.ReplyToMessage is { From: { } replyTo } ? await _membersStorage.GetOrCreate(replyTo) : null,
            message.Chat.Id.ToString()
        );
    }

    private static void ParseArguments(string text, ICommandListener commandListener, int argsStart,
        Dictionary<CommandArgument, string> arguments)
    {
        var currentArgumentStart = argsStart + 1;

        for (var i = 0; i < commandListener.Arguments.Count && currentArgumentStart < text.Length; i++)
        {
            var argument = commandListener.Arguments[i];

            switch (argument.ArgumentType)
            {
                case ArgumentType.Default:
                    var nextSpace = text.IndexOf(' ', currentArgumentStart + 1);
                    var value = nextSpace >= 0
                        ? text[currentArgumentStart..nextSpace]
                        : text[currentArgumentStart..];

                    arguments.Add(argument, value.Trim());
                    currentArgumentStart += value.Length;

                    break;

                case ArgumentType.Filler:
                    arguments.Add(argument, text[currentArgumentStart..].Trim());

                    return;
            }

            while (currentArgumentStart < text.Length && text[currentArgumentStart] == ' ')
            {
                currentArgumentStart++;
            }
        }
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