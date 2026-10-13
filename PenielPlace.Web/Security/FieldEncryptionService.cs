using System.Globalization;
using Microsoft.AspNetCore.DataProtection;

namespace PenielPlace.Web.Security;

public interface IFieldEncryptionService
{
    string? Protect(string? plaintext);
    string? Unprotect(string? protectedValue);
    string? ProtectAmount(decimal? amount);
    decimal? UnprotectAmount(string? protectedValue);
}

/// <summary>
/// Uses ASP.NET Core Data Protection (authenticated encryption) for sensitive fields.
/// The Data Protection key ring must be kept persistent and access-restricted in production.
/// </summary>
public sealed class FieldEncryptionService : IFieldEncryptionService
{
    private readonly IDataProtector _protector;

    public FieldEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("PenielPlace.SensitiveFields.v1");
    }

    public string? Protect(string? plaintext)
    {
        if (plaintext is null) return null;
        return _protector.Protect(plaintext);
    }

    public string? Unprotect(string? protectedValue)
    {
        if (protectedValue is null) return null;
        return _protector.Unprotect(protectedValue);
    }

    public string? ProtectAmount(decimal? amount)
    {
        if (amount is null) return null;
        return _protector.Protect(amount.Value.ToString("0.00", CultureInfo.InvariantCulture));
    }

    public decimal? UnprotectAmount(string? protectedValue)
    {
        if (protectedValue is null) return null;
        var raw = _protector.Unprotect(protectedValue);
        return decimal.Parse(raw, NumberStyles.Number, CultureInfo.InvariantCulture);
    }
}
