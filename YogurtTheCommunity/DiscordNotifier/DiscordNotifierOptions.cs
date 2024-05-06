namespace YogurtTheCommunity.DiscordNotifier;

public class DiscordNotifierOptions
{
    public bool Enabled { get; set; } = true;

    public string Token { get; set; } = null!;

    public float WaitBetweenJoins { get; set; } = 0;

    public float WaitBetweenStreaming { get; set; } = 0;

    public float WaitBeforeStatusDelete { get; set; } = 60;

    public long TelegramTargetId { get; set; }

    public int? TelegramThreadId { get; set; } = null;

    public bool NeedToPinMessage { get; set; } = true;
}