using System.ComponentModel.DataAnnotations;

namespace PenielPlaceMinistriesCSharp.Models;

public static class DonationCategories
{
    public const string Tithe = "tithe";
    public const string Offering = "offering";
    public const string OnceOffGift = "once_off_gift";
    public const string InKind = "in_kind";

    public static readonly string[] All =
    [
        Tithe,
        Offering,
        OnceOffGift,
        InKind
    ];

    public static string Label(string value) => value switch
    {
        Tithe => "Tithe",
        Offering => "Offering",
        OnceOffGift => "Once-off gift",
        InKind => "In-kind donation",
        _ => value
    };
}

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required]
    public string Role { get; set; } = "volunteer";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Donor
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    [EmailAddress, MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(40)]
    public string? Phone { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Donation> Donations { get; set; } = [];
}

public class Donation
{
    public int Id { get; set; }

    public int? DonorId { get; set; }
    public Donor? Donor { get; set; }

    [Required]
    public string Category { get; set; } = "";

    [Required]
    public string Kind { get; set; } = "monetary";

    public long? AmountCents { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateOnly ReceivedOn { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Receipt? Receipt { get; set; }
}

public class Receipt
{
    public int Id { get; set; }

    public int DonationId { get; set; }
    public Donation? Donation { get; set; }

    [Required, MaxLength(40)]
    public string ReceiptNumber { get; set; } = "";

    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
}