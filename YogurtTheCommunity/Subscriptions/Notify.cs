using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Services;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Subscriptions;

public class NotifyCommand : ICommandListener
{
    private readonly SubscriptionsStorage _subscriptionsStorage;
    private readonly MembersStorage _membersStorage;

    public string Command => "notify";

    public string Description => "notifies everyone from subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

    public NotifyCommand(SubscriptionsStorage subscriptionsStorage, MembersStorage membersStorage)
    {
        _subscriptionsStorage = subscriptionsStorage;
        _membersStorage = membersStorage;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var subscription = commandContext.GetArgument(Arguments[0]).ToLowerInvariant();

        if (string.IsNullOrEmpty(subscription))
        {
            await commandContext.Reply("Invalid subscription");

            return;
        }

        var subscribers = await _subscriptionsStorage.GetSubscribers(subscription);
        var members = await Task.WhenAll(subscribers.Select(_membersStorage.GetMemberById));
        var subscribersStrings = members
            .Where(m => m is not null)
            .Select(m => $"{{{{ mention \"{m!.Id}\" \"{m.Name.Escape()}\" }}}}")
            .ToArray();
        
        await commandContext.Reply($"A-hoy: {string.Join(", ", subscribersStrings)}");
        
    }
}