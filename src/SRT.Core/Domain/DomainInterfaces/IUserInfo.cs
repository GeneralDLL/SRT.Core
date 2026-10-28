using SRT.Core.SystemEnums;

namespace SRT.Core.Domain.DomainInterfaces
{
    public interface IUserInfo : IDatabaseEntryID
    {
        string GetFullName();
        UserRoleType userRole { get; }
        bool isSuperAdmin { get; }
        IReadOnlyList<UserRoleGrant> roleGrants { get; }
    }
}
