using Microsoft.Extensions.Options;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Options;

namespace YogurtTheCommunity.Services;

public class PermissionsManager
{
    private readonly IOptions<PermissionsOptions> _permissionsOptions;

    public PermissionsManager(IOptions<PermissionsOptions> permissionsOptions)
    {
        _permissionsOptions = permissionsOptions;
    }
    
    public string[] GetRolePermissions(string role) =>
        _permissionsOptions.Value.RolesPermissions.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<string>();

    public bool HasPermissions(MemberInfo memberInfo, params string[] permissions) =>
        permissions.All(permission =>
            memberInfo.Roles.Any(role => GetRolePermissions(role).Contains(permission))
        );
}