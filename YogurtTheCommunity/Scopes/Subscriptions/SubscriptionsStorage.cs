using StackExchange.Redis;

namespace YogurtTheCommunity.Subscriptions;

public class SubscriptionsStorage(IConnectionMultiplexer redis)
{
    public async Task<string[]> GetSubscriptions()
    {
        var db = redis.GetDatabase();

        return (await db.SetMembersAsync("community:subscriptions")).Select(c => (string)c!).ToArray();
    }

    public async Task<bool> Subscribe(Guid id, string subscription)
    {
        var db = redis.GetDatabase();

        await db.SetAddAsync("community:subscriptions", subscription);

        return await db.SetAddAsync($"community:subscriptions:{subscription}", id.ToString());
    }

    public async Task Unsubscribe(Guid id, string subscription)
    {
        var db = redis.GetDatabase();

        await db.SetRemoveAsync($"community:subscriptions:{subscription}", id.ToString());
    }

    public async Task<Guid[]> GetSubscribers(string subscription)
    {
        var db = redis.GetDatabase();

        return (await db.SetMembersAsync($"community:subscriptions:{subscription}"))
            .Select(c => Guid.Parse(c!))
            .ToArray();
    }

    public async Task<string[]> GetUserSubscriptions(Guid id)
    {
        var subscriptions = await GetSubscriptions();
        var userSubscriptions = new List<string>(subscriptions.Length);

        foreach (var subscription in subscriptions)
        {
            var subscribers = await GetSubscribers(subscription);

            if (subscribers.Contains(id))
            {
                userSubscriptions.Add(subscription);
            }
        }

        return userSubscriptions.ToArray();
    }
}