namespace SRT.Core.Domain.DomainInterfaces
{
    public sealed class UserRoleGrant
    {
        public string code { get; set; } = string.Empty;
        public IReadOnlyList<string> accesses { get; set; } = [];
    }
}
