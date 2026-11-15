namespace SRT.Core.Domain.DomainInterfaces
{
    public sealed class UserRoleGrant
    {
        public string code { get; set; } = string.Empty;
        public string? title { get; set; }
        public IReadOnlyList<string> accesses { get; set; } = [];
    }
}
