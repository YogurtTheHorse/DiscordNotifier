using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.TitleGiver;

public class SyncTitleCommand(TitlesStorage titlesStorage, TitlesManager titlesManager) : ICommandListener
{
    public string Command => "syncTitle";

    public string Description => "Syncs chat title of user";

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public async Task Execute(CommandContext commandContext)
    {
        var memberInfo = commandContext.ReplyTo ?? commandContext.MemberInfo;
        var title = await titlesStorage.GetTitle(memberInfo.Id);

        if (string.IsNullOrEmpty(title))
        {
            await commandContext.Reply("User has no title");
            return;
        }

        await titlesManager.UpdateTitle(memberInfo.Id, title);
        await commandContext.Reply("Done");
    }
}