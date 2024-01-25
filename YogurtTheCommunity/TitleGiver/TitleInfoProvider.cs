using YogurtTheCommunity.Abstractions;

namespace YogurtTheCommunity.TitleGiver;

public class TitleInfoProvider : IInfoProvider
{
    private readonly TitlesStorage _titlesStorage;

    public TitleInfoProvider(TitlesStorage titlesStorage)
    {
        _titlesStorage = titlesStorage;
    }

    public async Task<Dictionary<string, string>> GetInfo(Guid id) => new() {
        {
            "Title", await _titlesStorage.GetTitle(id) ?? "No title"
        }
    };
}