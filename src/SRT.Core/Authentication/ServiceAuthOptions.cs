namespace SRT.Core.Authentication;

public sealed class ServiceAuthOptions
{
    public const string SectionName = "ServiceAuth";

    public string Issuer { get; set; } = string.Empty;
    public int ClockSkewSeconds { get; set; }
    public int MaxTokenLifetimeMinutes { get; set; }
    public int FutureIssuedAtAllowanceSeconds { get; set; }
    public int KeysRefreshIntervalMinutes { get; set; }
    public int KeysRefreshMinIntervalSeconds { get; set; }
}
