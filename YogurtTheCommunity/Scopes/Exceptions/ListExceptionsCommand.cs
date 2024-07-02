using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.Scopes.Exceptions;

public class ListExceptionsCommand(ExceptionsStorage exceptions) : ICommandListener
{
    public string Command => "listExceptions";
    public string Description => "Lists exceptions in chat";

    public IList<CommandArgument> Arguments { get; } = [];

    public async Task Execute(CommandContext commandContext)
    {
        var chatId = long.Parse(commandContext.ChatId);
        var exceptionsList = await exceptions.GetExceptionsForTelegramChatId(chatId);

        if (exceptionsList.Count == 0)
        {
            await commandContext.Reply("No exceptions in chat");
            return;
        }

        await commandContext.Reply($"Exceptions in chat:\n{string.Join(", ", exceptions)}");
    }
}