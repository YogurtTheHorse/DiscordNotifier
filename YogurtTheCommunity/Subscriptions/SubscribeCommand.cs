using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.Subscriptions;

public class SubscribeCommand : ICommandListener
{
    private readonly SubscriptionsStorage _subscriptionsStorage;

    public string Command => "subscribe";

    public string Description => "adds user to subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

    public SubscribeCommand(SubscriptionsStorage subscriptionsStorage)
    {
        _subscriptionsStorage = subscriptionsStorage;
    }

    public async Task Execute(CommandContext commandContext)
    {
        var subscription = commandContext.GetArgument(Arguments[0]).ToLowerInvariant();

        if (string.IsNullOrEmpty(subscription))
        {
            await commandContext.Reply("Invalid subscription");

            return;
        }
        
        var added = await _subscriptionsStorage.Subscribe(commandContext.MemberInfo.Id, subscription);
        
        await commandContext.Reply(added ? "Ok" : "Error");
    }
}