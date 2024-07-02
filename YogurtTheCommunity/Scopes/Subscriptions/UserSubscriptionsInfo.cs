using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.Subscriptions;

public class UserSubscriptionsInfo(SubscriptionsStorage subscriptionsStorage) : IInfoProvider
{
    public async Task<Dictionary<string, string>> GetInfo(Guid id)
    {
        var subscriptions = await subscriptionsStorage.GetUserSubscriptions(id);

        return new Dictionary<string, string>() {
            {
                "subscriptions", string.Join(", ", subscriptions)
            }
        };
    }
}