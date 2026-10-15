using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PenielPlaceMinistriesCSharp.Models;
using PenielPlaceMinistriesCSharp.Services;

namespace PenielPlaceMinistriesCSharp.Controllers;

[Authorize(Roles = "admin")]
public class ReportsController(
    IReportingService reports,
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
                await reports.ReportAsync(
                    from, to, category, donorId));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(
                await reports.ReportAsync(null, null, null, null));
        }
    }
}