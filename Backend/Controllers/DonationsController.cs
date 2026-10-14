using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PenielPlaceMinistriesCSharp.Models;
using PenielPlaceMinistriesCSharp.Services;

namespace PenielPlaceMinistriesCSharp.Controllers;

[Authorize]
public class DonationsController(
    IDonationService donations,
    IDonorService donors) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? from,
        DateOnly? to,
        string? category,
        int? donorId)
    {
        try
        {
            ViewBag.Donors = await donors.AllAsync();
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.Category = category;
            ViewBag.DonorId = donorId;

            return View(
                await donations.AllAsync(from, to, category, donorId));
        }
        catch (ArgumentException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Donors = await donors.AllAsync();

        return View(new DonationRequest
        {
            ReceivedOn = DateOnly.FromDateTime(DateTime.Today),
            Kind = "monetary",
            Category = DonationCategories.Offering
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonationRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Donors = await donors.AllAsync();
            return View(request);
        }

        try
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var donation = await donations.CreateAsync(request, userId);

            TempData["Success"] =
                "Donation saved and receipt generated.";

            return RedirectToAction(
                "Show",
                "Receipts",
                new { id = donation.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Donors = await donors.AllAsync();
            return View(request);
        }
        catch (Exception)
        {
            ModelState.AddModelError(
                string.Empty,
                "Donation could not be saved. Please try again.");

            ViewBag.Donors = await donors.AllAsync();
            return View(request);
        }
    }
}