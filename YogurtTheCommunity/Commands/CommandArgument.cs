namespace YogurtTheCommunity.Commands;

public record CommandArgument(string Name, string DefaultValue = "", ArgumentType ArgumentType = ArgumentType.Default);

public enum ArgumentType
{
    Default,
    Filler
}