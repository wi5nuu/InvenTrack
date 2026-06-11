namespace InvenTrack.Shared.Constants;

/// <summary>Centralized application constants for roles, policies, caching, and limits.</summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";
    }

    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireManagerOrAbove = "RequireManagerOrAbove";
    }

    public static class CacheKeys
    {
        public const string ProductCategories = "ProductCategories";
        public const string Suppliers = "Suppliers";
        public const string DashboardSummary = "DashboardSummary";
        public const string FailedLoginAttempts = "FailedLoginAttempts_";
        public const string RateLimit = "RateLimit_";
    }

    public static class Limits
    {
        public const int MaxFileSizeMb = 10;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        public const int AuthLockoutAttempts = 5;
        public const int LockoutMinutes = 15;
        public const int AccessTokenExpirationMinutes = 15;
        public const int RefreshTokenExpirationDays = 7;
        public const int ApiRateLimitPerMinute = 100;
        public const int WebRateLimitPerMinute = 200;
    }

    public static class Import
    {
        public const int BatchSize = 100;
        public const string UploadPath = "uploads/imports";
        public static readonly string[] AllowedExtensions = { ".csv", ".xlsx" };
    }
}
