using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PenielPlace.Web.Services;
using PenielPlace.Web.ViewModels;

namespace PenielPlace.Web.Controllers;

[Authorize(Roles = "Leadership")]
public sealed class DashboardController : Controller
{
    private readonly DonationReportingService _reporting;

    public DashboardController(DonationReportingService reporting) => _reporting = reporting;

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null)
    {
        var start = from?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = to?.Date ?? DateTime.Today;

        if (!ValidRange(start, end))
        {
            ModelState.AddModelError(string.Empty, "Choose a date range from 1 January 2000 through today, with the start date no later than the end date.");
            return View(new DashboardViewModel { From = start, To = end });
        }

        var facts = (await _reporting.GetAllAsync())
            .Where(d => d.Date.Date >= start && d.Date.Date <= end)
            .ToList();

        var model = new DashboardViewModel
        {
            From = start,
            To = end,
            TotalMonetary = facts.Sum(d => d.Amount ?? 0m),
            DonationCount = facts.Count,
            InKindCount = facts.Count(d => d.IsInKind),
            DonorCount = facts.Where(d => d.DonorId.HasValue && !d.IsAnonymous).Select(d => d.DonorId!.Value).Distinct().Count(),
            ByCategory = facts.GroupBy(d => d.CategoryName)
                .Select(g => new CategorySummary(g.Key, g.Sum(d => d.Amount ?? 0m), g.Count()))
                .OrderByDescending(g => g.Total).ThenBy(g => g.CategoryName).ToList(),
            ByMonth = facts.GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                .Select(g => new MonthSummary(g.Key, g.Sum(d => d.Amount ?? 0m)))
                .OrderBy(g => g.Month).ToList(),
            TopDonors = facts.Where(d => d.DonorId.HasValue && !d.IsAnonymous)
                .GroupBy(d => new { Id = d.DonorId!.Value, d.DonorName })
                .Select(g => new DonorSummary(g.Key.Id, g.Key.DonorName, g.Count(), g.Sum(d => d.Amount ?? 0m)))
                .OrderByDescending(g => g.Total).ThenBy(g => g.DonorName).Take(10).ToList(),
            Recent = facts.OrderByDescending(d => d.Date).ThenByDescending(d => d.DonationId).Take(10)
                .Select(d => new DonationRow
                {
                    DonationId = d.DonationId,
                    Date = d.Date,
                    DonorName = d.DonorName,
                    CategoryName = d.CategoryName,
                    IsInKind = d.IsInKind,
                    Amount = d.Amount,
                    InKindDescription = d.InKindDescription,
                    ReceiptId = d.ReceiptId,
                    ReceiptNumber = d.ReceiptNumber
                }).ToList()
        };

        return View(model);
    }

    private static bool ValidRange(DateTime from, DateTime to) =>
        from.Date >= new DateTime(2000, 1, 1) && to.Date <= DateTime.Today && from.Date <= to.Date;
}
