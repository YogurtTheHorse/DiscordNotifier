using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.TitleGiver;

public class TitleInfoProvider(TitlesStorage titlesStorage) : IInfoProvider
{
    public async Task<Dictionary<string, string>> GetInfo(Guid id) => new() {
        {
            "Title", await titlesStorage.GetTitle(id) ?? "No title"
        }
    };
}