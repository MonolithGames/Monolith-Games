using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Monolith.Web.Data;
using Monolith.Web.Models;

namespace Monolith.Web.Services;

public sealed class CoinbaseSettingsStore(
    IDbContextFactory<MonolithDbContext> dbFactory,
    IDataProtectionProvider protectionProvider)
{
    private readonly IDataProtector protector = protectionProvider.CreateProtector("Monolith.Coinbase.PrivateKey.v1");

    public CoinbaseSettingsStatus GetStatus()
    {
        using var db = dbFactory.CreateDbContext();
        var credentials = db.CoinbaseCredentials.AsNoTracking().OrderBy(item => item.Id).FirstOrDefault();
        return credentials is null
            ? new(false, null, false, false)
            : new(true, credentials.ApiKeyName, credentials.LiveTradingEnabled, credentials.WithdrawalsEnabled);
    }

    public void Save(CoinbaseSettingsInput input)
    {
        if (string.IsNullOrWhiteSpace(input.ApiKeyName) || string.IsNullOrWhiteSpace(input.PrivateKey))
            throw new InvalidOperationException("An API key name and private key are required.");

        using var db = dbFactory.CreateDbContext();
        var credentials = db.CoinbaseCredentials.OrderBy(item => item.Id).FirstOrDefault() ?? new CoinbaseCredentialEntity { Id = 1 };
        credentials.ApiKeyName = input.ApiKeyName.Trim();
        credentials.ProtectedPrivateKey = protector.Protect(input.PrivateKey.Trim());
        credentials.LiveTradingEnabled = input.LiveTradingEnabled;
        credentials.WithdrawalsEnabled = false;
        credentials.UpdatedAtUtc = DateTime.UtcNow;
        db.CoinbaseCredentials.Update(credentials);
        db.SaveChanges();
    }

    public (string ApiKeyName, string PrivateKey)? GetCredentialMaterial()
    {
        using var db = dbFactory.CreateDbContext();
        var credentials = db.CoinbaseCredentials.AsNoTracking().OrderBy(item => item.Id).FirstOrDefault();
        if (credentials is null)
            return null;

        return (credentials.ApiKeyName, protector.Unprotect(credentials.ProtectedPrivateKey));
    }
}