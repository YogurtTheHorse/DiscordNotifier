using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Subscriptions;

public class UnSubscribeCommand : ICommandListener
{
    private readonly SubscriptionsStorage _subscriptionsStorage;
    private readonly PermissionsManager _permissionsManager;

    public string Command => "unsubscribe";

    public string Description => "removes user from subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

    public UnSubscribeCommand(SubscriptionsStorage subscriptionsStorage, PermissionsManager permissionsManager)
    {
        _subscriptionsStorage = subscriptionsStorage;
        _permissionsManager = permissionsManager;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var subscription = commandContext.GetArgument(Arguments[0]).ToLowerInvariant();

        if (string.IsNullOrEmpty(subscription))
        {
            await commandContext.Reply("Invalid subscription");

            return;
        }

        var member = commandContext.ReplyTo ?? commandContext.MemberInfo;

        if (member != commandContext.MemberInfo
            && !_permissionsManager.HasPermissions(commandContext.MemberInfo, "subscriptions.edit.others"))
        {
            await commandContext.Reply("You don't have permissions to unsubscribe others");

            return;
        }
        
        await _subscriptionsStorage.Unsubscribe(member.Id, subscription);
        
        await commandContext.Reply("Ok");
    }
}