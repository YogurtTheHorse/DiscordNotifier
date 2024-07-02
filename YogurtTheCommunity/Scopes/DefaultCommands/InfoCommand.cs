using YogurtTheCommunity.Abstractions;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Commands.DefaultCommands;

public class InfoCommand(IEnumerable<IInfoProvider> infoProviders) : ICommandListener
{
    public string Command => "info";

    public string Description => "sends info about user";

    public string[] RequiredPermissions { get; } =
    [
        "base.info.read"
    ];

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public async Task Execute(CommandContext commandContext)
    {
        var member = commandContext.ReplyTo ?? commandContext.MemberInfo;
        var baseInfo =
            $"{{{{bold \"Id\"}}}}: {member.Id}\n"
            + $"{{{{bold \"Name\"}}}}: {member.Name.Escape()}\n"
            + $"{{{{bold \"Roles\"}}}}: {string.Join(", ", member.Roles)}";

        var info = (await Task.WhenAll(
            infoProviders
                .Select(async x =>
                {
                    var values = await x.GetInfo(member.Id);

                    return string.Join(
                        "\n",
                        values.Select(v => $"{{{{bold \"{v.Key.Escape()}\"}}}}: {v.Value}")
                    );
                })
        )).Where(x => !string.IsNullOrEmpty(x));

        await commandContext.Reply($"{baseInfo}\n\n{string.Join("\n\n", info)}");
    }
}