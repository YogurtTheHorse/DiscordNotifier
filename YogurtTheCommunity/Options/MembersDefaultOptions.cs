namespace YogurtTheCommunity.Options;

public class MembersDefaultOptions
{
    public Dictionary<long, DefaultMember> TelegramDefaultIds { get; set; } = new();

    public string[] DefaultRoles { get; set; } = Array.Empty<string>();

    public class DefaultMember
    {
        public Guid DefaultId { get; set; }

        public string[] DefaultRoles { get; set; } = Array.Empty<string>();
    }
}