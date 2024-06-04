namespace YogurtTheCommunity.Data;

public record MemberInfo(Guid Id, string Name, string[] Roles);

public record ExtraMemberInfo(string Description);