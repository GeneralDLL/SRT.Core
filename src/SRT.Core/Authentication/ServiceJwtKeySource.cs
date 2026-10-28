namespace SRT.Core.Authentication;

public sealed class ServiceJwtPublicKey
{
    public const string Es256 = "ES256";

    public string Kid { get; init; } = string.Empty;
    public string Alg { get; init; } = Es256;
    public string PublicKeySpki { get; init; } = string.Empty;
    public bool Active { get; init; }
}

public sealed class ServiceJwtKeySet
{
    public bool Succeeded { get; init; }
    public IReadOnlyList<ServiceJwtPublicKey> Keys { get; init; } = [];

    public static ServiceJwtKeySet Success(IReadOnlyList<ServiceJwtPublicKey> keys)
        => new() { Succeeded = true, Keys = keys };

    public static ServiceJwtKeySet Failed()
        => new() { Succeeded = false };
}

public interface IServiceJwtKeySource
{
    Task<ServiceJwtKeySet> GetAsync(CancellationToken cancellationToken = default);
}

public interface IServiceTokenClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemServiceTokenClock : IServiceTokenClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public enum ServiceJwtKeyLookupKind
{
    Found,
    Unknown,
    Unavailable
}

public sealed class ServiceJwtKeyLookup
{
    public ServiceJwtKeyLookupKind Kind { get; init; }
    public ServiceJwtPublicKey? Key { get; init; }

    public static ServiceJwtKeyLookup Found(ServiceJwtPublicKey key) => new() { Kind = ServiceJwtKeyLookupKind.Found, Key = key };
    public static ServiceJwtKeyLookup Unknown() => new() { Kind = ServiceJwtKeyLookupKind.Unknown };
    public static ServiceJwtKeyLookup Unavailable() => new() { Kind = ServiceJwtKeyLookupKind.Unavailable };
}
