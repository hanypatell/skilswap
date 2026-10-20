namespace SkillSwap.Filters
{
    public static class SessionKeys
    {
        public const string UserId = "UserId";
        public const string UserName = "UserName";
    }

    public static class SessionExtensions
    {
        public static int? GetUserId(this ISession session) => session.GetInt32(SessionKeys.UserId);

        public static string? GetUserName(this ISession session) => session.GetString(SessionKeys.UserName);
    }
}
