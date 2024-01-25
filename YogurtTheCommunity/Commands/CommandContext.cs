using YogurtTheCommunity.Data;

namespace YogurtTheCommunity.Commands;

public class CommandContext
{
    private readonly Dictionary<CommandArgument, string> _argumentValues;

    public Func<string, Task> Reply { get; }

    public MemberInfo MemberInfo { get; }

    public MemberInfo? ReplyTo { get; }

    public CommandContext(Dictionary<CommandArgument, string> argumentValues, Func<string, Task> reply, MemberInfo memberInfo, MemberInfo? replyTo)
    {
        _argumentValues = argumentValues;
        
        Reply = reply;
        MemberInfo = memberInfo;
        ReplyTo = replyTo;
    }

    public string GetArgument(CommandArgument argument) =>
        _argumentValues.TryGetValue(argument, out var value)
            ? value
            : argument.DefaultValue;
}