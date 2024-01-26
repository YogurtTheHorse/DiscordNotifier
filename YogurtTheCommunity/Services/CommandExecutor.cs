using YogurtTheCommunity.Commands;
using YogurtTheCommunity.Utils;

namespace YogurtTheCommunity.Services;

public class CommandExecutor
{
    private readonly PermissionsManager _permissionsManager;
    private readonly ILogger<CommandExecutor> _logger;

    public CommandExecutor(PermissionsManager permissionsManager, ILogger<CommandExecutor> logger)
    {
        _permissionsManager = permissionsManager;
        _logger = logger;
    }

    public async Task Execute(ICommandListener commandListener, CommandContext commandContext)
    {
        if (!_permissionsManager.HasPermissions(commandContext.MemberInfo, commandListener.RequiredPermissions))
        {
            var roles = string.Join(", ", commandListener.RequiredPermissions);
            await commandContext.Reply($"You don't have permissions to execute this command ({roles})");

            return;
        }

        try
        {
            await commandListener.Execute(commandContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command {command}", commandListener.Command);
            await commandContext.Reply($"Error executing command: {ex.Message.Escape()}");
        }
    }
}