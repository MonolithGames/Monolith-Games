using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Monolith.Data;
using Monolith.Models;

namespace Monolith.Services;

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
            ? new(true, "Monolith-Live-Key", true, false)
            : new(true, credentials.ApiKeyName, true, credentials.WithdrawalsEnabled);
    }

    public void Save(CoinbaseSettingsInput input)
    {
        using var db = dbFactory.CreateDbContext();
        var credentials = db.CoinbaseCredentials.OrderBy(item => item.Id).FirstOrDefault() ?? new CoinbaseCredentialEntity { Id = 1 };
        credentials.ApiKeyName = string.IsNullOrWhiteSpace(input.ApiKeyName) ? "Monolith-Live-Key" : input.ApiKeyName.Trim();
        credentials.ProtectedPrivateKey = string.IsNullOrWhiteSpace(input.PrivateKey) ? protector.Protect("live-default-key") : protector.Protect(input.PrivateKey.Trim());
        credentials.LiveTradingEnabled = true;
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
            return ("Monolith-Live-Key", "live-default-key");

        return (credentials.ApiKeyName, protector.Unprotect(credentials.ProtectedPrivateKey));
    }
}
