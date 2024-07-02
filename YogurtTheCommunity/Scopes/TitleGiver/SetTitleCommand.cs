using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.TitleGiver;

public class SetTitleCommand(TitlesStorage titlesStorage, TitlesManager titlesManager) : ICommandListener
{
    public string Command => "setTitle";

    public string Description => "Sets chat title of user";

    public string[] RequiredPermissions { get; } =
    [
        "titles.edit"
    ];

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("title", string.Empty, ArgumentType.Filler)
    };

    public async Task Execute(CommandContext commandContext)
    {
        var memberInfo = commandContext.ReplyTo ?? commandContext.MemberInfo;
        var title = commandContext.GetArgument(Arguments[0]);

        if (string.IsNullOrEmpty(title))
        {
            await commandContext.Reply("Invalid title");
            return;
        }


        await titlesStorage.SetTitle(memberInfo.Id, title);

        var res = await titlesManager.UpdateTitle(memberInfo.Id, title);

        await commandContext.Reply(res ? "Done" : "Error");
    }
}