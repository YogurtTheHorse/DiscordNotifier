using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Scopes.ExtraInfo;

public class SetDescriptionCommand : ICommandListener
{
    private readonly MembersStorage _members;

    public string Command => "setDescription";

    public string Description => "sets member description";

    public string[] RequiredPermissions { get; } =
    {
        "base.info.edit-own"
    };

    public IList<CommandArgument> Arguments { get; } = new[]
    {
        new CommandArgument("description", string.Empty, ArgumentType.Filler)
    };

    public SetDescriptionCommand(MembersStorage members) => _members = members;

    public async Task Execute(CommandContext commandContext)
    {
        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid description");

            return;
        }

        // extremely unoptimal
        var extraInfo = await _members.GetExtraInfo(commandContext.MemberInfo.Id) with
        {
            Description = commandContext.GetArgument(Arguments[0])
        };

        await _members.SetExtraInfo(commandContext.MemberInfo.Id, extraInfo);
        await commandContext.Reply("Ok");
    }
}