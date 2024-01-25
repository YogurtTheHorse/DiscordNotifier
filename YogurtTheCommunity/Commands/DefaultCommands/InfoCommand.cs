namespace YogurtTheCommunity.Commands.DefaultCommands;

public class InfoCommand : ICommandListener
{
    public string Command => "info";

    public string Description => "sends info about user";

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public async Task Execute(CommandContext commandContext)
    {
        var member = commandContext.ReplyTo ?? commandContext.MemberInfo;
        
        await commandContext.Reply(
            $"Your id: {member.Id}\n"
            + $"Name: {member.Name}\n"
            + $"Your roles: {string.Join(", ", member.Roles)}"
        );
    }
}