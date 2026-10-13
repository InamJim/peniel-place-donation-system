using System.ComponentModel.DataAnnotations;

namespace PenielPlace.Web.Models;

/// <summary>
/// Donor PII and donation amounts/notes are stored only in protected columns.
/// Values are decrypted by application services after a record is loaded.
/// </summary>
public sealed class Donor
{
    public int DonorId { get; set; }

    [Required, MaxLength(1024)]
    public string FullNameProtected { get; set; } = string.Empty;

    [MaxLength(1024)]
    public string? EmailProtected { get; set; }

    [MaxLength(512)]
    public string? PhoneProtected { get; set; }

    [MaxLength(2048)]
    public string? NotesProtected { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<DonationRecord> Donations { get; set; } = new List<DonationRecord>();
}

public sealed class DonationCategory
{
    public int DonationCategoryId { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public bool IsInKind { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DonationRecord> Donations { get; set; } = new List<DonationRecord>();
}

public sealed class DonationRecord
{
    public int DonationId { get; set; }

    public DateTime Date { get; set; }

    public int DonationCategoryId { get; set; }
    public DonationCategory Category { get; set; } = null!;

    public int? DonorId { get; set; }
    public Donor? Donor { get; set; }
    public bool IsAnonymous { get; set; }

    // Amounts, descriptions, payment method and notes are encrypted strings.
    // This prevents database-side SUM/LIKE operations; reporting decrypts records in memory.
    [MaxLength(256)]
    public string? AmountProtected { get; set; }

    [MaxLength(1024)]
    public string? InKindDescriptionProtected { get; set; }

    [MaxLength(256)]
    public string? PaymentMethodProtected { get; set; }

    [MaxLength(2048)]
    public string? NotesProtected { get; set; }

    [Required, MaxLength(40)]
    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
