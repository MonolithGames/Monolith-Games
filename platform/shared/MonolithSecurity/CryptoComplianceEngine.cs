namespace Monolith.Security;

using System;
using System.Security.Cryptography;
using System.Text;

public sealed class CryptoComplianceEngine
{
    public static string GenerateZeroTrustSignature(string payload, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    public static bool VerifyZeroTrustSignature(string payload, string secretKey, string signature)
    {
        string expected = GenerateZeroTrustSignature(payload, secretKey);
        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }
}
