using System.ComponentModel.DataAnnotations;

namespace PenielPlace.Web.ViewModels;

public class CategoryTotal { public string CategoryName { get; set; } = ""; public int Count { get; set; } public decimal Total { get; set; } }
public class PeriodTotal   { public DateTime Month { get; set; } public decimal Total { get; set; } }
public class DonorTotal    { public int DonorId { get; set; } public string DonorName { get; set; } = ""; public int Count { get; set; } public decimal Total { get; set; } }

public class DashboardViewModel : IValidatableObject
{
    [DataType(DataType.Date)] public DateTime From { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [DataType(DataType.Date)] public DateTime To { get; set; } = DateTime.Today;

    public decimal TotalMonetary { get; set; }
    public int DonationCount { get; set; }
    public int InKindCount { get; set; }
    public int DonorCount { get; set; }

    public IReadOnlyList<CategoryTotal> ByCategory { get; set; } = Array.Empty<CategoryTotal>();
    public IReadOnlyList<PeriodTotal> ByMonth { get; set; } = Array.Empty<PeriodTotal>();
    public IReadOnlyList<DonorTotal> TopDonors { get; set; } = Array.Empty<DonorTotal>();
    public IReadOnlyList<DonationRow> Recent { get; set; } = Array.Empty<DonationRow>();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (To < From)
            yield return new ValidationResult("The end date must be on or after the start date.", new[] { nameof(To) });
    }
}

public class ReportRow { public string Label { get; set; } = ""; public int Count { get; set; } public decimal Total { get; set; } }

public class ReportViewModel : IValidatableObject
{
    [DataType(DataType.Date)] public DateTime From { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [DataType(DataType.Date)] public DateTime To { get; set; } = DateTime.Today;

    /// <summary>"category", "donor" or "month".</summary>
    [RegularExpression("^(category|donor|month)$", ErrorMessage = "Choose how to group the report.")]
    public string GroupBy { get; set; } = "category";

    public IReadOnlyList<ReportRow> Rows { get; set; } = Array.Empty<ReportRow>();
    public decimal GrandTotal { get; set; }
    public int GrandCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (To < From)
            yield return new ValidationResult("The end date must be on or after the start date.", new[] { nameof(To) });
    }
}

public class ReceiptViewModel
{
    public int ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = "";
    public DateTime IssuedDate { get; set; }
    public string DonorName { get; set; } = "Anonymous";
    public DateTime DonationDate { get; set; }
    public string CategoryName { get; set; } = "";
    public bool IsInKind { get; set; }
    public decimal? Amount { get; set; }
    public string? InKindDescription { get; set; }
    public string? PaymentMethod { get; set; }
    public string IssuedBy { get; set; } = "";
}
