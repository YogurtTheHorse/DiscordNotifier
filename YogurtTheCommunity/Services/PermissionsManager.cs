using Microsoft.Extensions.Options;
using YogurtTheCommunity.Data;
using YogurtTheCommunity.Options;

namespace YogurtTheCommunity.Services;

public class PermissionsManager(IOptions<PermissionsOptions> permissionsOptions)
{
    public string[] GetRolePermissions(string role) =>
        permissionsOptions.Value.RolesPermissions.TryGetValue(role, out var permissions)
            ? permissions
            : [];

    public bool HasPermissions(MemberInfo memberInfo, params string[] permissions) =>
        permissions.All(permission =>
            memberInfo.Roles.Any(role => GetRolePermissions(role).Contains(permission))
        );
}