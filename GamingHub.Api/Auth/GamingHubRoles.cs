namespace GamingHub.Api.Auth
{
    public static class GamingHubRoles
    {
        public const string Admin = "Admin";
        public const string Member = "Member";

        public static readonly IReadOnlyCollection<string> All = new[] { Admin, Member };
    }
}
