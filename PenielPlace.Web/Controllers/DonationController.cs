using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PenielPlace.Web.Data;
using PenielPlace.Web.Models;
using PenielPlace.Web.Security;
using PenielPlace.Web.ViewModels;

namespace PenielPlace.Web.Controllers;

[Authorize(Roles = "Volunteer,Leadership")]
public sealed class DonationController : Controller
{
    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cash", "EFT / bank transfer", "Card", "Other"
    };

    private readonly ApplicationDbContext _db;
    private readonly IFieldEncryptionService _encryption;
    private readonly ILogger<DonationController> _logger;

    public DonationController ( ApplicationDbContext db, IFieldEncryptionService encryption, ILogger<DonationController> logger )
    {
        _db = db;
        _encryption = encryption;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Create ( int? donorId = null )
    {
        var model = new DonationCreateViewModel { DonationDate = DateTime.Today };
        if ( donorId is > 0 )
        {
            var donor = await _db.Donors.AsNoTracking().SingleOrDefaultAsync(d => d.DonorId == donorId.Value);
            if ( donor is not null )
            {
                model.DonorId = donor.DonorId;
                model.DonorName = _encryption.Unprotect(donor.FullNameProtected);
            }
        }

        await LoadCategoriesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create ( DonationCreateViewModel model )
    {
        var category = model.CategoryId is > 0
            ? await _db.DonationCategories.AsNoTracking().SingleOrDefaultAsync(c => c.DonationCategoryId == model.CategoryId && c.IsActive)
            : null;

        if ( category is null )
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Choose a valid type of gift.");
        }
        else
        {
            model.IsInKind = category.IsInKind;
            // The view model validates fields conditionally. Remove only the conditional field
            // errors that do not apply to the actual database category, then validate below.
            if ( category.IsInKind )
                ModelState.Remove(nameof(model.Amount));
            else
                ModelState.Remove(nameof(model.InKindDescription));
        }

        Donor? donor = null;
        if ( model.IsAnonymous )
        {
            ModelState.Remove(nameof(model.DonorId));
            model.DonorId = null;
            model.DonorName = null;
        }
        else if ( model.DonorId is > 0 )
        {
            donor = await _db.Donors.AsNoTracking().SingleOrDefaultAsync(d => d.DonorId == model.DonorId.Value);
            if ( donor is null )
            {
                ModelState.AddModelError(nameof(model.DonorId), "That donor could not be found. Choose a donor again.");
            }
            else
            {
                model.DonorName = _encryption.Unprotect(donor.FullNameProtected);
            }
        }
        else
        {
            ModelState.AddModelError(nameof(model.DonorId), "Pick a donor, or tick 'Anonymous or donor not known'.");
        }

        if ( model.DonationDate is null || model.DonationDate.Value.Date > DateTime.Today || model.DonationDate.Value.Date < new DateTime(2000, 1, 1) )
            ModelState.AddModelError(nameof(model.DonationDate), "Choose a valid date between 1 January 2000 and today.");

        if ( !string.IsNullOrWhiteSpace(model.PaymentMethod) && !AllowedPaymentMethods.Contains(model.PaymentMethod.Trim()) )
            ModelState.AddModelError(nameof(model.PaymentMethod), "Choose a valid payment method.");

        if ( category?.IsInKind == true && string.IsNullOrWhiteSpace(model.InKindDescription) )
            ModelState.AddModelError(nameof(model.InKindDescription), "Describe what was donated.");
        if ( category is { IsInKind: false } && (model.Amount is null || model.Amount < 0.01m || model.Amount > 10_000_000m) )
            ModelState.AddModelError(nameof(model.Amount), "Enter an amount between R 0.01 and R 10 000 000.");

        if ( !ModelState.IsValid || category is null )
        {
            await LoadCategoriesAsync(model);
            return View(model);
        }

        var now = DateTime.UtcNow;
        var record = new DonationRecord
        {
            Date = model.DonationDate!.Value.Date,
            DonationCategoryId = category.DonationCategoryId,
            DonorId = model.IsAnonymous ? null : donor!.DonorId,
            IsAnonymous = model.IsAnonymous,
            AmountProtected = category.IsInKind ? null : _encryption.ProtectAmount(model.Amount),
            InKindDescriptionProtected = category.IsInKind ? _encryption.Protect(model.InKindDescription!.Trim()) : null,
            PaymentMethodProtected = category.IsInKind ? null : _encryption.Protect(EmptyToNull(model.PaymentMethod)),
            NotesProtected = _encryption.Protect(EmptyToNull(model.Notes)),
            ReceiptNumber = $"PP-{now:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 40).ToUpperInvariant(),
            CreatedUtc = now
        };

        _db.Donations.Add(record);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch ( DbUpdateException ex )
        {
            _logger.LogError(ex, "Could not save donation record");
            ModelState.AddModelError(string.Empty, "The donation could not be saved. Please check the details and try again.");
            await LoadCategoriesAsync(model);
            return View(model);
        }

        TempData["Success"] = "Donation recorded and receipt issued.";
        return RedirectToAction("Details", "Receipt", new { id = record.DonationId });
    }

    private async Task LoadCategoriesAsync ( DonationCreateViewModel model )
    {
        model.Categories = await _db.DonationCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DonationCategoryId)
            .Select(c => new CategoryOption(c.DonationCategoryId, c.Name, c.IsInKind))
            .ToListAsync();
    }

    private static string? EmptyToNull ( string? value )
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
