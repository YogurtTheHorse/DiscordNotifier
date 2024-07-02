using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Subscriptions;

public class NotifyCommand(SubscriptionsStorage subscriptionsStorage, MembersStorage membersStorage)
    : ICommandListener
{
    public string Command => "notify";

    public string Description => "notifies everyone from subscription";

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

        var subscribers = await subscriptionsStorage.GetSubscribers(subscription);
        var members = await Task.WhenAll(subscribers.Select(membersStorage.GetMemberById));
        var subscribersStrings = members
            .Where(m => m is not null)
            .Select(m => $"{{{{ mention \"{m!.Id}\" \"{m.Name.Escape()}\" }}}}")
            .ToArray();
        
        await commandContext.Reply($"A-hoy: {string.Join(", ", subscribersStrings)}");
        
    }
}