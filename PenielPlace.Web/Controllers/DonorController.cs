using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PenielPlace.Web.Data;
using PenielPlace.Web.Models;
using PenielPlace.Web.Security;
using PenielPlace.Web.ViewModels;

namespace PenielPlace.Web.Controllers;

[Authorize(Roles = "Volunteer,Leadership")]
public sealed class DonorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IFieldEncryptionService _encryption;
    private readonly ILogger<DonorController> _logger;

    public DonorController ( ApplicationDbContext db, IFieldEncryptionService encryption, ILogger<DonorController> logger )
    {
        _db = db;
        _encryption = encryption;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index ( [FromQuery] DonorLookupViewModel model )
    {
        if ( !ModelState.IsValid )
        {
            model.Results = Array.Empty<DonorListItem>();
            return View(model);
        }

        var query = model.Query?.Trim();
        model.HasSearched = !string.IsNullOrWhiteSpace(query);
        model.Results = model.HasSearched
            ? await BuildDonorListAsync(query!)
            : Array.Empty<DonorListItem>();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search ( string? term )
    {
        term = term?.Trim();
        if ( string.IsNullOrWhiteSpace(term) || term.Length < 2 )
            return Json(Array.Empty<object>());
        if ( term.Length > 100 )
            return BadRequest(new { error = "Search text must be 100 characters or fewer." });

        // Protected names/contact details cannot be filtered by SQL LIKE. Decrypt only after
        // authorization, then filter the small starter dataset in memory.
        var donors = await _db.Donors.AsNoTracking().OrderBy(d => d.DonorId).ToListAsync();
        var matches = donors.Select(d => new
        {
            id = d.DonorId,
            name = _encryption.Unprotect(d.FullNameProtected) ?? "Donor",
            email = _encryption.Unprotect(d.EmailProtected),
            phone = _encryption.Unprotect(d.PhoneProtected)
        })
            .Where(d => d.name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (d.email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (d.phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(8)
            .Select(d => new { d.id, d.name, detail = string.IsNullOrWhiteSpace(d.phone) ? d.email : d.phone })
            .ToList();

        return Json(matches);
    }

    [HttpGet]
    public IActionResult Create ( string? returnUrl = null ) => View(new DonorFormViewModel
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create ( DonorFormViewModel model )
    {
        model.FullName = model.FullName?.Trim() ?? string.Empty;
        model.Email = EmptyToNull(model.Email);
        model.Phone = EmptyToNull(model.Phone);
        model.Notes = EmptyToNull(model.Notes);
        model.ReturnUrl = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : null;

        if ( !ModelState.IsValid )
            return View(model);

        var donor = new Donor
        {
            FullNameProtected = _encryption.Protect(model.FullName)!,
            EmailProtected = _encryption.Protect(model.Email),
            PhoneProtected = _encryption.Protect(model.Phone),
            NotesProtected = _encryption.Protect(model.Notes),
            CreatedUtc = DateTime.UtcNow
        };

        _db.Donors.Add(donor);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch ( DbUpdateException ex )
        {
            _logger.LogError(ex, "Could not save donor record");
            ModelState.AddModelError(string.Empty, "The donor could not be saved. Check the details and try again.");
            return View(model);
        }

        TempData["Success"] = "Donor saved.";
        if ( model.ReturnUrl is not null )
            return Redirect(model.ReturnUrl);

        return RedirectToAction(nameof(Details), new { id = donor.DonorId });
    }

    [HttpGet]
    public async Task<IActionResult> Details ( int id )
    {
        if ( id <= 0 ) return NotFound();

        var donor = await _db.Donors.AsNoTracking().SingleOrDefaultAsync(d => d.DonorId == id);
        if ( donor is null ) return NotFound();

        var donations = await _db.Donations.AsNoTracking()
            .Include(d => d.Category)
            .Where(d => d.DonorId == id)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.DonationId)
            .ToListAsync();

        var rows = donations.Select(d => new DonationRow
        {
            DonationId = d.DonationId,
            Date = d.Date,
            DonorName = _encryption.Unprotect(donor.FullNameProtected) ?? "Donor",
            CategoryName = d.Category.Name,
            IsInKind = d.Category.IsInKind,
            Amount = _encryption.UnprotectAmount(d.AmountProtected),
            InKindDescription = _encryption.Unprotect(d.InKindDescriptionProtected),
            ReceiptId = d.DonationId,
            ReceiptNumber = d.ReceiptNumber
        }).ToList();

        return View(new DonorDetailsViewModel
        {
            DonorId = donor.DonorId,
            FullName = _encryption.Unprotect(donor.FullNameProtected) ?? "Donor",
            Email = _encryption.Unprotect(donor.EmailProtected),
            Phone = _encryption.Unprotect(donor.PhoneProtected),
            Notes = _encryption.Unprotect(donor.NotesProtected),
            TotalGiven = rows.Where(d => !d.IsInKind).Sum(d => d.Amount ?? 0m),
            Donations = rows
        });
    }

    private async Task<IReadOnlyList<DonorListItem>> BuildDonorListAsync ( string query )
    {
        var donors = await _db.Donors.AsNoTracking().ToListAsync();
        var donations = await _db.Donations.AsNoTracking().ToListAsync();

        var list = donors.Select(d =>
        {
            var name = _encryption.Unprotect(d.FullNameProtected) ?? "Donor";
            var email = _encryption.Unprotect(d.EmailProtected);
            var phone = _encryption.Unprotect(d.PhoneProtected);
            var related = donations.Where(x => x.DonorId == d.DonorId).ToList();
            return new DonorListItem
            {
                DonorId = d.DonorId,
                FullName = name,
                Email = email,
                Phone = phone,
                DonationCount = related.Count,
                TotalGiven = related.Sum(x => _encryption.UnprotectAmount(x.AmountProtected) ?? 0m),
                LastDonationDate = related.Count == 0 ? null : related.Max(x => (DateTime?) x.Date)
            };
        })
        .Where(d => d.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (d.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || (d.Phone?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
        .OrderBy(d => d.FullName, StringComparer.OrdinalIgnoreCase)
        .Take(200)
        .ToList();

        return list;
    }

    private static string? EmptyToNull ( string? value )
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
