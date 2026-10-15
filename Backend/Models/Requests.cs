using System.ComponentModel.DataAnnotations;

namespace PenielPlaceMinistriesCSharp.Models;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(1)]
    public string Password { get; set; } = "";
}

public class DonorRequest
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public class DonationRequest
{
    public int? DonorId { get; set; }

    [Required]
    public string Category { get; set; } = "";

    [Required]
    public string Kind { get; set; } = "monetary";

    public decimal? Amount { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateOnly? ReceivedOn { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}