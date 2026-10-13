using Microsoft.EntityFrameworkCore;
using PenielPlace.Web.Data;
using PenielPlace.Web.Security;

namespace PenielPlace.Web.Services;

public sealed record DonationFact(
    int DonationId,
    DateTime Date,
    int DonationCategoryId,
    string CategoryName,
    int? DonorId,
    string DonorName,
    bool IsAnonymous,
    bool IsInKind,
    decimal? Amount,
    string? InKindDescription,
    string? PaymentMethod,
    int? ReceiptId,
    string ReceiptNumber);

/// <summary>
/// Loads and decrypts donation records for reporting. This intentionally performs monetary
/// aggregation in application memory because the financial amounts are encrypted at rest.
/// For a high-volume deployment, use SQL Server Always Encrypted or another design that
/// supports protected aggregate queries rather than loading the full dataset for every report.
/// </summary>
public sealed class DonationReportingService
{
    private readonly ApplicationDbContext _db;
    private readonly IFieldEncryptionService _encryption;

    public DonationReportingService(ApplicationDbContext db, IFieldEncryptionService encryption)
    {
        _db = db;
        _encryption = encryption;
    }

    public async Task<IReadOnlyList<DonationFact>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _db.Donations
            .AsNoTracking()
            .Include(d => d.Category)
            .Include(d => d.Donor)
            .OrderByDescending(d => d.Date)
            .ToListAsync(cancellationToken);

        return records.Select(d => new DonationFact(
            d.DonationId,
            d.Date,
            d.DonationCategoryId,
            d.Category.Name,
            d.DonorId,
            d.IsAnonymous || d.Donor is null ? "Anonymous" : (_encryption.Unprotect(d.Donor.FullNameProtected) ?? "Donor"),
            d.IsAnonymous,
            d.Category.IsInKind,
            _encryption.UnprotectAmount(d.AmountProtected),
            _encryption.Unprotect(d.InKindDescriptionProtected),
            _encryption.Unprotect(d.PaymentMethodProtected),
            d.DonationId,
            d.ReceiptNumber)).ToList();
    }
}
