using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;

namespace YogurtTheCommunity.Subscriptions;

public class SubscribeCommand(SubscriptionsStorage subscriptionsStorage, PermissionsManager permissionsManager)
    : ICommandListener
{
    public string Command => "subscribe";

    public string Description => "adds user to subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

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
            && !permissionsManager.HasPermissions(commandContext.MemberInfo, "subscriptions.edit.others"))
        {
            await commandContext.Reply("You don't have permissions to subscribe others");

            return;
        }

        var added = await subscriptionsStorage.Subscribe(member.Id, subscription);

        await commandContext.Reply(added ? $"Subscribed {member.Name} for {subscription}" : "Error");
    }
}