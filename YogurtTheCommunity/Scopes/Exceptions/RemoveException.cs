using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.Scopes.Exceptions;

public class RemoveException(ExceptionsStorage exceptions) : ICommandListener
{
    public string Command => "removeException";
    public string Description => "Removes exception from chat";

    public IList<CommandArgument> Arguments { get; } =
    [
        new CommandArgument("exceptionName", string.Empty, ArgumentType.Filler)
    ];
    public async Task Execute(CommandContext commandContext)
    {
        var exceptionName = commandContext.GetArgument(Arguments[0]);
        
        if (string.IsNullOrEmpty(exceptionName))
        {
            await commandContext.Reply("Invalid exception name");
            return;
        }
        
        var chatId = long.Parse(commandContext.ChatId);

        await exceptions.RemoveExceptionFromTelegramChat(exceptionName, chatId);
        await commandContext.Reply($"Removed exception '{exceptionName}' to chat");
    }
}