using SRT.Core.Domain.DomainInterfaces;

namespace SRT.Core.Authentication
{
    public enum DashboardAccessDecision
    {
        Allowed = 1,
        ActiveRoleRequired = 2,
        ActiveRoleUnknown = 3,
        Forbidden = 4
    }

    public static class DashboardAccess
    {
        public const string HeaderName = "X-Active-Role";
        public const string ActiveRoleItemKey = "ActiveRole";
        public const string ForbiddenCode = "FORBIDDEN_ACCESS";
        public const string ActiveRoleRequiredCode = "ACTIVE_ROLE_REQUIRED";
        public const string ActiveRoleUnknownCode = "ACTIVE_ROLE_UNKNOWN";

        public static DashboardAccessDecision Decide(
            IUserInfo user,
            string? requiredAccess,
            IReadOnlyList<string>? headerValues,
            out string? activeRole)
        {
            activeRole = null;
            if (user.isSuperAdmin || string.IsNullOrWhiteSpace(requiredAccess))
                return DashboardAccessDecision.Allowed;

            if (headerValues is null || headerValues.Count != 1)
                return DashboardAccessDecision.ActiveRoleRequired;

            var raw = headerValues[0];
            if (string.IsNullOrWhiteSpace(raw) || raw.Contains(','))
                return DashboardAccessDecision.ActiveRoleRequired;

            UserRoleGrant? grant = null;
            if (user.roleGrants is not null)
            {
                foreach (var item in user.roleGrants)
                {
                    if (item is not null && string.Equals(item.code, raw, StringComparison.Ordinal))
                    {
                        grant = item;
                        break;
                    }
                }
            }

            if (grant is null)
                return DashboardAccessDecision.ActiveRoleUnknown;

            activeRole = grant.code;
            if (grant.accesses is null)
                return DashboardAccessDecision.Forbidden;

            foreach (var access in grant.accesses)
            {
                if (string.Equals(access, requiredAccess, StringComparison.Ordinal))
                    return DashboardAccessDecision.Allowed;
            }

            return DashboardAccessDecision.Forbidden;
        }
    }
}
