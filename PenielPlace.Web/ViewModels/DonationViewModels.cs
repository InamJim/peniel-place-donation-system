using System.ComponentModel.DataAnnotations;

namespace PenielPlace.Web.ViewModels;

/// <summary>One row of the DonationCategory table (Tithe, Offering, Once-off gift, In kind).</summary>
public record CategoryOption(int Id, string Name, bool IsInKind);

public class DonationCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Choose the type of gift.")]
    [Range(1, int.MaxValue, ErrorMessage = "Choose a valid type of gift.")]
    public int? CategoryId { get; set; }

    // This is a UI hint only. DonationController always uses the category row from the database.
    public bool IsInKind { get; set; }

    [Range(typeof(decimal), "0.01", "10000000", ErrorMessage = "Enter an amount between R 0.01 and R 10 000 000.")]
    public decimal? Amount { get; set; }

    [StringLength(200, ErrorMessage = "Describe the items in 200 characters or fewer.")]
    [Display(Name = "What was donated")]
    public string? InKindDescription { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a valid donor.")]
    public int? DonorId { get; set; }

    // Display convenience only. The controller never treats a posted name as authoritative.
    public string? DonorName { get; set; }
    public bool IsAnonymous { get; set; }

    [Required(ErrorMessage = "Choose the date of the gift.")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? DonationDate { get; set; } = DateTime.Today;

    [StringLength(40)]
    public string? PaymentMethod { get; set; }

    [StringLength(250, ErrorMessage = "Notes must be 250 characters or fewer.")]
    public string? Notes { get; set; }

    public IReadOnlyList<CategoryOption> Categories { get; set; } = Array.Empty<CategoryOption>();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (IsInKind)
        {
            if (string.IsNullOrWhiteSpace(InKindDescription))
                yield return new ValidationResult("Describe what was donated, like '5 bags of maize meal'.", new[] { nameof(InKindDescription) });
        }
        else if (Amount is null)
        {
            yield return new ValidationResult("Enter the amount received.", new[] { nameof(Amount) });
        }

        if (DonorId is null && !IsAnonymous)
            yield return new ValidationResult("Pick a donor, or tick 'Anonymous or donor not known'.", new[] { nameof(DonorId) });

        if (DonationDate is not null && DonationDate.Value.Date > DateTime.Today)
            yield return new ValidationResult("The date cannot be in the future.", new[] { nameof(DonationDate) });
    }
}
