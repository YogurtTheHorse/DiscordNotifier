using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.TitleGiver;

public class SyncTitleCommand : ICommandListener
{
    private readonly TitlesStorage _titlesStorage;
    private readonly TitlesManager _titlesManager;

    public string Command => "syncTitle";

    public string Description => "Syncs chat title of user";

    public IList<CommandArgument> Arguments => Array.Empty<CommandArgument>();

    public SyncTitleCommand(TitlesStorage titlesStorage, TitlesManager titlesManager)
    {
        _titlesStorage = titlesStorage;
        _titlesManager = titlesManager;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var memberInfo = commandContext.ReplyTo ?? commandContext.MemberInfo;
        var title = await _titlesStorage.GetTitle(memberInfo.Id);

        if (string.IsNullOrEmpty(title))
        {
            await commandContext.Reply("User has no title");
            return;
        }

        await _titlesManager.UpdateTitle(memberInfo.Id, title);
        await commandContext.Reply("Done");
    }
}