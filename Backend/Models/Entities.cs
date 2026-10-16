using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PenielPlaceMinistriesCSharp.Models;

public static class Roles
{
    public const string Volunteer = "Volunteer";
    public const string Leadership = "Leadership";
    public const string Staff = Volunteer + "," + Leadership;
    public static readonly string[] All = [Volunteer, Leadership];
}
public static class PaymentMethods
{
    public static readonly string[] All = ["Cash", "EFT / bank transfer", "Card", "Other"];

    public static bool IsValid ( string? value ) =>
        !string.IsNullOrWhiteSpace(value) &&
        All.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}

public sealed class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }
}

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

    public string? CreatedById { get; set; }

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


    [MaxLength(256)]
    public string? AmountProtected { get; set; }
    [MaxLength(1024)]
    public string? InKindDescriptionProtected { get; set; }
    [MaxLength(256)]
    public string? PaymentMethodProtected { get; set; }
    [MaxLength(2048)]
    public string? NotesProtected { get; set; }
    public string? CreatedById { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsVoided { get; set; }
    public DateTime? VoidedUtc { get; set; }
    public string? VoidedById { get; set; }
    [MaxLength(200)]
    public string? VoidedByName { get; set; }
    [MaxLength(1024)]
    public string? VoidReasonProtected { get; set; }
    public Receipt? Receipt { get; set; }
}

public sealed class Receipt
{
    public int ReceiptId { get; set; }
    public int DonationId { get; set; }
    public DonationRecord? Donation { get; set; }
    [Required, MaxLength(40)]
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    [MaxLength(200)]
    public string IssuedBy { get; set; } = string.Empty;
}
