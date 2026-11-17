namespace MythByte.Shared.Security;

public static class Permissions
{
    public const string Player = Roles.Player + "," + Roles.Moderator + "," + Roles.Admin;
    public const string Moderator = Roles.Moderator + "," + Roles.Admin;
    public const string Admin = Roles.Admin;
}
