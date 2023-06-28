namespace DiscordNotifier.Options;

public class TelegramOptions
{
    public string Token { get; set; } = null!;

    public long TargetId { get; set; }
}