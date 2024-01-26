using YogurtTheCommunity.Commands;

namespace YogurtTheCommunity.Subscriptions;

public class UnSubscribeCommand : ICommandListener
{
    private readonly SubscriptionsStorage _subscriptionsStorage;

    public string Command => "unsubscribe";

    public string Description => "removes user from subscription";

    public IList<CommandArgument> Arguments { get; } = new[] {
        new CommandArgument("subscription", string.Empty, ArgumentType.Filler)
    };

    public UnSubscribeCommand(SubscriptionsStorage subscriptionsStorage)
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
        
        await _subscriptionsStorage.Unsubscribe(commandContext.MemberInfo.Id, subscription);
        
        await commandContext.Reply("Ok");
    }
}