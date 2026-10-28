using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SRT.Core.Authentication;

public sealed class ServiceJwtDocument
{
    public string Alg { get; init; } = string.Empty;
    public string Kid { get; init; } = string.Empty;
    public string? HeaderType { get; init; }
    public string Issuer { get; init; } = string.Empty;
    public IReadOnlyList<string> Audiences { get; init; } = [];
    public string Subject { get; init; } = string.Empty;
    public string Jti { get; init; } = string.Empty;
    public string? PayloadType { get; init; }
    public long IssuedAt { get; init; }
    public long Expires { get; init; }
    public long? NotBefore { get; init; }
    public byte[] SigningInput { get; init; } = [];
    public byte[] Signature { get; init; } = [];
}

public static class ServiceJwtReader
{
    public static bool TryRead(string token, out ServiceJwtDocument document)
    {
        document = null!;
        var parts = token.Split('.');
        if (parts.Length != 3)
            return false;

        try
        {
            using var header = JsonDocument.Parse(Base64Url(parts[0]));
            using var payload = JsonDocument.Parse(Base64Url(parts[1]));
            var signature = Base64Url(parts[2]);
            var headerRoot = header.RootElement;
            var payloadRoot = payload.RootElement;
            if (!TryString(headerRoot, "alg", out var alg) || !TryString(headerRoot, "kid", out var kid))
                return false;

            TryString(headerRoot, "typ", out var headerType);
            TryString(payloadRoot, "iss", out var issuer);
            TryString(payloadRoot, "sub", out var subject);
            TryString(payloadRoot, "typ", out var payloadType);
            TryString(payloadRoot, "jti", out var jti);
            if (!TryInt64(payloadRoot, "iat", out var issuedAt) || !TryInt64(payloadRoot, "exp", out var expires))
                return false;

            long? notBefore = null;
            if (TryInt64(payloadRoot, "nbf", out var nbf))
                notBefore = nbf;

            document = new ServiceJwtDocument
            {
                Alg = alg ?? string.Empty,
                Kid = kid ?? string.Empty,
                HeaderType = headerType,
                Issuer = issuer ?? string.Empty,
                Audiences = ReadAudiences(payloadRoot),
                Subject = subject ?? string.Empty,
                Jti = jti ?? string.Empty,
                PayloadType = payloadType,
                IssuedAt = issuedAt,
                Expires = expires,
                NotBefore = notBefore,
                SigningInput = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]),
                Signature = signature
            };
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool Verify(ServiceJwtDocument document, string publicKeySpki)
    {
        byte[] spki;
        try
        {
            spki = Convert.FromBase64String(publicKeySpki);
        }
        catch (FormatException)
        {
            return false;
        }

        using var ecdsa = ECDsa.Create();
        try
        {
            ecdsa.ImportSubjectPublicKeyInfo(spki, out _);
        }
        catch (Exception)
        {
            return false;
        }

        if (!IsP256(ecdsa))
            return false;

        return ecdsa.VerifyData(document.SigningInput, document.Signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
    }

    private static bool IsP256(ECDsa ecdsa)
    {
        if (ecdsa.KeySize != 256)
            return false;

        var oid = ecdsa.ExportParameters(false).Curve.Oid;
        return string.Equals(oid.Value, "1.2.840.10045.3.1.7", StringComparison.Ordinal)
            || string.Equals(oid.FriendlyName, "nistP256", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> ReadAudiences(JsonElement payload)
    {
        if (!payload.TryGetProperty("aud", out var aud))
            return [];

        if (aud.ValueKind == JsonValueKind.String)
            return [aud.GetString() ?? string.Empty];

        if (aud.ValueKind != JsonValueKind.Array)
            return [];

        var list = new List<string>();
        foreach (var item in aud.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
                list.Add(item.GetString() ?? string.Empty);
        }

        return list;
    }

    private static bool TryString(JsonElement element, string name, out string? value)
    {
        value = null;
        if (!element.TryGetProperty(name, out var property))
            return false;
        if (property.ValueKind != JsonValueKind.String)
            return false;
        value = property.GetString();
        return true;
    }

    private static bool TryInt64(JsonElement element, string name, out long value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var property))
            return false;
        if (property.ValueKind == JsonValueKind.Number)
            return property.TryGetInt64(out value);
        return property.ValueKind == JsonValueKind.String
            && long.TryParse(property.GetString(), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static byte[] Base64Url(string text)
    {
        var padded = text.Replace('-', '+').Replace('_', '/');
        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded
        };
        return Convert.FromBase64String(padded);
    }
}
