using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class AddRoleCommand(MembersStorage members) : ICommandListener
{
    public string Command => "addRole";

    public string Description => "adds role to user";

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

        await members.AddRole(member.Id, commandContext.GetArgument(Arguments[0]));
        await commandContext.Reply("Ok");
    }
}