using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class SetNameCommand(MembersStorage members) : ICommandListener
{
    public string Command => "setName";

    public string Description => "sets member name in system";
    
    public string[] RequiredPermissions { get; } =
    [
        "base.info.edit-own"
    ];

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("name", string.Empty, ArgumentType.Filler)
    };

    public async Task Execute(CommandContext commandContext)
    {
        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid name");

            return;
        }

        await members.SetName(commandContext.MemberInfo.Id, commandContext.GetArgument(Arguments[0]));
        await commandContext.Reply("Ok");
    }
}