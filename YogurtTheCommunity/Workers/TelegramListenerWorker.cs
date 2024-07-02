using Scriban;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Workers;

public class TelegramListenerWorker(
    ITelegramBotClient botClient,
    IEnumerable<ICommandListener> commandListeners,
    IEnumerable<ITelegramUpdateListener> updateListeners,
    MembersStorage membersStorage,
    CommandExecutor commandExecutor,
    ILogger<TelegramListenerWorker> logger)
    : BackgroundService
{
    private readonly ICommandListener[] _commandListeners = commandListeners.ToArray();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        botClient.StartReceiving(
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

        await botClient.SetMyCommandsAsync(
            commands,
            cancellationToken: stoppingToken
        );
    }

    private Task OnPollingError(ITelegramBotClient client, Exception exception, CancellationToken cts)
    {
        logger.LogError(exception, "Telegram polling error");

        return Task.CompletedTask;
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken cts)
    {
        await InternalUpdateProcess(update, cts);

        foreach (var listener in updateListeners)
        {
            try
            {
                await listener.OnUpdate(client, update, cts);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in update listener {listener}", listener.GetType().Name);
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

        await commandExecutor.Execute(commandListener, commandContext);
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
            Reply,
            await membersStorage.GetOrCreate(message.From!),
            message.ReplyToMessage is { From: { } replyTo } ? await membersStorage.GetOrCreate(replyTo) : null,
            message.Chat.Id.ToString(),
            message.ReplyToMessage is { MessageId: var id } ? id.ToString() : null
        );

        async Task Reply(string text)
        {
            var context = GetParserContext();
            var template = Template.Parse(text);

            string? result;

            try
            {
                result = await template.RenderAsync(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rendering template");

                return;
            }

            if (!string.IsNullOrEmpty(result))
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    result,
                    parseMode: ParseMode.Html
                );
            }
        }
    }

    private TemplateContext GetParserContext()
    {
        var messageFunctions = new TelegramMessageFunctions(membersStorage);
        var context = new TemplateContext(messageFunctions);

        return context;
    }

    private static void ParseArguments(
        string text,
        ICommandListener commandListener,
        int argsStart,
        Dictionary<CommandArgument, string> arguments
    )
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