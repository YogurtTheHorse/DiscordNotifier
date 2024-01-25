namespace YogurtTheCommunity.Abstractions;

public interface IInfoProvider
{
    Task<Dictionary<string, string>> GetInfo(Guid id);
}