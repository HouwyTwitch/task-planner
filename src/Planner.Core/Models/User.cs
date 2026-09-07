namespace Planner.Core.Models;

public sealed class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string Role { get; set; } = UserRoles.User;
    public string RoleDisplay => UserRoles.ToRussian(Role);

    public override string ToString() => DisplayName;
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public static string ToRussian(string role) => role == Admin ? "Администратор" : "Пользователь";
}
