using YogurtTheCommunity.Data;

namespace YogurtTheCommunity.Commands;

public class CommandContext
{
    private readonly Dictionary<CommandArgument, string> _argumentValues;

    public Func<string, Task> Reply { get; }

    public MemberInfo MemberInfo { get; }

    public string ChatId { get; }

    public MemberInfo? ReplyTo { get; }
    
    public string? ReplyToMessageId { get; } // todo: fix that shit and make it abstract...

    public CommandContext(Dictionary<CommandArgument, string> argumentValues, Func<string, Task> reply, MemberInfo memberInfo, MemberInfo? replyTo, string chatId, string? replyToMessageId)
    {
        _argumentValues = argumentValues;
        
        Reply = reply;
        MemberInfo = memberInfo;
        ReplyTo = replyTo;
        ChatId = chatId;
        ReplyToMessageId = replyToMessageId;
    }

    public string GetArgument(CommandArgument argument) =>
        _argumentValues.TryGetValue(argument, out var value)
            ? value
            : argument.DefaultValue;
}