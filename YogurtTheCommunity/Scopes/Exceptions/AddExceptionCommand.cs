using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.Scopes.Exceptions;

public class AddExceptionCommand(ExceptionsStorage exceptions) : ICommandListener
{
    public string Command => "addException";
    public string Description => "Adds exception to chat";

    public IList<CommandArgument> Arguments { get; } = new[]
    {
        new CommandArgument("exceptionName", string.Empty, ArgumentType.Filler)
    };

    public async Task Execute(CommandContext commandContext)
    {
        var exceptionName = commandContext.GetArgument(Arguments[0]);
        
        if (string.IsNullOrEmpty(exceptionName))
        {
            await commandContext.Reply("Invalid exception name");
            return;
        }
        
        var chatId = long.Parse(commandContext.ChatId);

        await exceptions.AddExceptionForTelegramChat(exceptionName, chatId);
        await commandContext.Reply($"Added exception '{exceptionName}' to chat");
    }
}