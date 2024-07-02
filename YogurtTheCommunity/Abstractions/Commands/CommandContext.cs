using YogurtTheCommunity.Data;

namespace YogurtTheCommunity.Commands;

public class CommandContext(
    Dictionary<CommandArgument, string> argumentValues,
    Func<string, Task> reply,
    MemberInfo memberInfo,
    MemberInfo? replyTo,
    string chatId,
    string? replyToMessageId)
{
    public Func<string, Task> Reply { get; } = reply;

    public MemberInfo MemberInfo { get; } = memberInfo;

    public string ChatId { get; } = chatId;

    public MemberInfo? ReplyTo { get; } = replyTo;

    public string? ReplyToMessageId { get; } = replyToMessageId; // todo: fix that shit and make it abstract...

    public string GetArgument(CommandArgument argument) =>
        argumentValues.TryGetValue(argument, out var value)
            ? value
            : argument.DefaultValue;
}