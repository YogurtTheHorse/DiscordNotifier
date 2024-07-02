namespace YogurtTheCommunity.DiscordNotifier;

public class DiscordNotifierOptions
{
    public bool Enabled { get; set; } = true;

    public string Token { get; set; } = null!;

    public float WaitBetweenJoins { get; set; }

    public float WaitBetweenStreaming { get; set; }

    public float WaitBeforeStatusDelete { get; set; } = 60;
    
    public long TelegramTargetId { get; set; }
}