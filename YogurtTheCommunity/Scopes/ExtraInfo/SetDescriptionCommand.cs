using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Scopes.ExtraInfo;

public class SetDescriptionCommand(MembersStorage members) : ICommandListener
{
    public string Command => "setDescription";

    public string Description => "sets member description";

    public string[] RequiredPermissions { get; } =
    [
        "base.info.edit-own"
    ];

    public IList<CommandArgument> Arguments { get; } = new[]
    {
        new CommandArgument("description", string.Empty, ArgumentType.Filler)
    };

    public async Task Execute(CommandContext commandContext)
    {
        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid description");

            return;
        }

        // extremely unoptimal
        var extraInfo = await members.GetExtraInfo(commandContext.MemberInfo.Id) with
        {
            Description = commandContext.GetArgument(Arguments[0])
        };

        await members.SetExtraInfo(commandContext.MemberInfo.Id, extraInfo);
        await commandContext.Reply("Ok");
    }
}