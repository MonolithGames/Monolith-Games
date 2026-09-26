using System.Security.Cryptography;
using System.Text;

namespace Monolith.Services;

public sealed class AccountService(IConfiguration configuration, IWebHostEnvironment environment)
{
    public const string UserName = "auora";

    public bool Validate(string userName, string password)
    {
        var configuredPassword = configuration["AUORA_PASSWORD"];
        if (string.IsNullOrEmpty(configuredPassword))
        {
            configuredPassword = "0"; // Default fallback password "0"
        }

        return string.Equals(userName, UserName, StringComparison.Ordinal) &&
               CryptographicOperations.FixedTimeEquals(
                   Encoding.UTF8.GetBytes(configuredPassword),
                   Encoding.UTF8.GetBytes(password));
    }

    public bool IsConfigured => true;

    public bool IsDevelopment => environment.IsDevelopment();
}
