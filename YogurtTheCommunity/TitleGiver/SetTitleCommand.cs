using Telegram.Bot;
using Telegram.Bot.Exceptions;
using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.TitleGiver;

public class SetTitleCommand : ICommandListener
{
    private readonly TitlesStorage _titlesStorage;
    private readonly TitlesManager _titlesManager;

    public string Command => "setTitle";

    public string Description => "Sets chat title of user";

    public string[] RequiredPermissions { get; } = {
        "titles.edit"
    };

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("title", string.Empty)
    };

    public SetTitleCommand(TitlesStorage titlesStorage, TitlesManager titlesManager)
    {
        _titlesStorage = titlesStorage;
        _titlesManager = titlesManager;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var memberInfo = commandContext.ReplyTo ?? commandContext.MemberInfo;

        if (string.IsNullOrEmpty(commandContext.GetArgument(Arguments[0])))
        {
            await commandContext.Reply("Invalid title");
        }

        var title = commandContext.GetArgument(Arguments[0]);

        await _titlesStorage.SetTitle(memberInfo.Id, title);

        var res = await _titlesManager.UpdateTitle(memberInfo.Id, title);

        await commandContext.Reply(res ? "Ok" : "Error");
    }
}