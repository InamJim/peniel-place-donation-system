using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PenielPlace.Web.Data;
using PenielPlace.Web.Security;
using PenielPlace.Web.ViewModels;

namespace PenielPlace.Web.Controllers;

[Authorize(Roles = "Volunteer,Leadership")]
public sealed class ReceiptController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IFieldEncryptionService _encryption;

    public ReceiptController ( ApplicationDbContext db, IFieldEncryptionService encryption )
    {
        _db = db;
        _encryption = encryption;
    }

    [HttpGet]
    public async Task<IActionResult> Details ( int id )
    {
        if ( id <= 0 ) return NotFound();

        var donation = await _db.Donations.AsNoTracking()
            .Include(d => d.Category)
            .Include(d => d.Donor)
            .SingleOrDefaultAsync(d => d.DonationId == id);

        if ( donation is null ) return NotFound();

        return View(new ReceiptViewModel
        {
            DonationId = donation.DonationId,
            ReceiptId = donation.DonationId,
            ReceiptNumber = donation.ReceiptNumber,
            DonorName = donation.IsAnonymous || donation.Donor is null
                ? "Anonymous"
                : (_encryption.Unprotect(donation.Donor.FullNameProtected) ?? "Donor"),
            DonationDate = donation.Date,
            CategoryName = donation.Category.Name,
            IsInKind = donation.Category.IsInKind,
            InKindDescription = _encryption.Unprotect(donation.InKindDescriptionProtected),
            PaymentMethod = _encryption.Unprotect(donation.PaymentMethodProtected),
            Amount = _encryption.UnprotectAmount(donation.AmountProtected),
            IssuedDate = donation.CreatedUtc,
            IssuedBy = User.Identity?.Name ?? "System"
        });
    }
}
