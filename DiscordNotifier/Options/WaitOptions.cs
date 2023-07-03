namespace DiscordNotifier.Options;

public class WaitOptions
{
    public float WaitBetweenJoins { get; set; } = 0;

    public float WaitBetweenStreaming { get; set; } = 0;

    public float WaitBeforeStatusDelete { get; set; } = 60;
}