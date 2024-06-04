using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Subscriptions;

public class SubscribeCommand : ICommandListener
{
    private readonly PermissionsManager _permissionsManager;
    private readonly SubscriptionsStorage _subscriptionsStorage;

    public string Command => "subscribe";

    public string Description => "adds user to subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

    public SubscribeCommand(SubscriptionsStorage subscriptionsStorage, PermissionsManager permissionsManager)
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
            await commandContext.Reply("You don't have permissions to subscribe others");

            return;
        }

        var added = await _subscriptionsStorage.Subscribe(member.Id, subscription);

        await commandContext.Reply(added ? $"Subscribed {member.Name} for {subscription}" : "Error");
    }
}