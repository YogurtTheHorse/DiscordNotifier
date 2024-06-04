using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.Subscriptions;

public class UserSubscriptionsInfo : IInfoProvider
{
    private readonly SubscriptionsStorage _subscriptionsStorage;

    public UserSubscriptionsInfo(SubscriptionsStorage subscriptionsStorage)
    {
        _subscriptionsStorage = subscriptionsStorage;
    }

    public async Task<Dictionary<string, string>> GetInfo(Guid id)
    {
        var subscriptions = await _subscriptionsStorage.GetUserSubscriptions(id);

        return new Dictionary<string, string>() {
            {
                "subscriptions", string.Join(", ", subscriptions)
            }
        };
    }
}