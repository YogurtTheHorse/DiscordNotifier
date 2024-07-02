using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class RemoveRoleCommand(MembersStorage members) : ICommandListener
{
    public string Command => "removeRole";

    public string Description => "removes role from user";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("role", string.Empty)
    };
    
    public string[] RequiredPermissions { get; } = {
        "roles.edit"
    };

    public async Task Execute(CommandContext commandContext)
    {
        var member = commandContext.ReplyTo ?? commandContext.MemberInfo;
        
        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid role");
        }

        await members.RemoveRole(member.Id, commandContext.GetArgument(Arguments[0]));
        await commandContext.Reply("Ok");
    }
}