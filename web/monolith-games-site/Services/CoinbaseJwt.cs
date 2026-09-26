using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Monolith.Services;

internal static class CoinbaseJwt
{
    public static string Create(string keyName, string privateKey, string method, string path)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKey);
        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyName };
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);
        var now = DateTimeOffset.UtcNow;
        var token = new JwtSecurityToken(
            issuer: "cdp",
            claims: new[]
            {
                new System.Security.Claims.Claim("sub", keyName),
                new System.Security.Claims.Claim("uri", $"{method} api.coinbase.com{path}")
            },
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(2).UtcDateTime,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
