using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class InfoCommand : ICommandListener
{
    private readonly IEnumerable<IInfoProvider> _infoProviders;

    public string Command => "info";

    public string Description => "sends info about user";

    public string[] RequiredPermissions { get; } = {
        "base.info.read"
    };

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public InfoCommand(IEnumerable<IInfoProvider> infoProviders)
    {
        _infoProviders = infoProviders;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var member = commandContext.ReplyTo ?? commandContext.MemberInfo;

        var baseInfo =
            $"Id: {member.Id}\n"
            + $"Name: {member.Name}\n"
            + $"Roles: {string.Join(", ", member.Roles)}";
        
        var info = (await Task.WhenAll(
            _infoProviders
                .Select(async x =>
                {
                    var values = await x.GetInfo(member.Id);

                    return string.Join("\n", values.Select(v => $"{v.Key}: {v.Value}"));
                })
            )).Where(x => !string.IsNullOrEmpty(x));
        
        await commandContext.Reply($"{baseInfo}\n\n{string.Join("\n\n", info)}");
    }
}