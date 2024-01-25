using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class SetNameCommand : ICommandListener
{
    private readonly MembersStorage _members;

    public string Command => "setName";

    public string Description => "sets member name in system";
    
    public string[] RequiredPermissions { get; } = {
        "base.info.edit-own"
    };

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("name", string.Empty)
    };

    public SetNameCommand(MembersStorage members)
    {
        _members = members;
    }

    public async Task Execute(CommandContext commandContext)
    {
        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid name");

            return;
        }

        await _members.SetName(commandContext.MemberInfo.Id, commandContext.GetArgument(Arguments[0]));
        await commandContext.Reply("Ok");
    }
}