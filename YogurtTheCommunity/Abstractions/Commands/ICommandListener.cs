namespace YogurtTheCommunity.Commands;

public interface ICommandListener
{
    string Command { get; }
    
    string Description { get; }
    
    string[] RequiredPermissions => [];
    
    IList<CommandArgument> Arguments { get; }

    Task Execute(CommandContext commandContext);
}