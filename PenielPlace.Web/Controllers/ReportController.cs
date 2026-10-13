using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PenielPlace.Web.Services;
using PenielPlace.Web.ViewModels;

namespace PenielPlace.Web.Controllers;

[Authorize(Roles = "Leadership")]
public sealed class ReportController : Controller
{
    private readonly DonationReportingService _reporting;

    public ReportController ( DonationReportingService reporting ) => _reporting = reporting;

    [HttpGet]
    public async Task<IActionResult> Index ( DateTime? from = null, DateTime? to = null, string groupBy = "category" )
    {
        var start = from?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var end = to?.Date ?? DateTime.Today;
        groupBy = (groupBy ?? "category").Trim().ToLowerInvariant();
        var model = new ReportViewModel { From = start, To = end, GroupBy = groupBy, GeneratedAt = DateTime.Now };

        if ( !ValidRange(start, end) || !IsAllowedGroup(groupBy) )
        {
            ModelState.AddModelError(string.Empty, "Choose a valid date range and grouping option.");
            return View(model);
        }

        var facts = (await _reporting.GetAllAsync())
            .Where(d => d.Date.Date >= start && d.Date.Date <= end)
            .ToList();
        model.Rows = BuildRows(facts, groupBy);
        model.GrandCount = facts.Count;
        model.GrandTotal = facts.Sum(d => d.Amount ?? 0m);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv ( DateTime? from, DateTime? to, string groupBy = "category" )
    {
        if ( from is null || to is null || !ValidRange(from.Value.Date, to.Value.Date) )
            return BadRequest("A valid date range is required.");

        groupBy = (groupBy ?? "category").Trim().ToLowerInvariant();
        if ( !IsAllowedGroup(groupBy) )
            return BadRequest("Choose a valid grouping option.");

        var facts = (await _reporting.GetAllAsync())
            .Where(d => d.Date.Date >= from.Value.Date && d.Date.Date <= to.Value.Date)
            .ToList();
        var rows = BuildRows(facts, groupBy);

        var csv = new StringBuilder();
        csv.AppendLine("Group,Gift count,Money received");
        foreach ( var row in rows )
            csv.Append(CsvCell(row.Label)).Append(',').Append(row.Count).Append(',')
                .Append(row.Total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).AppendLine();
        csv.Append("Total,").Append(facts.Count).Append(',')
            .Append(facts.Sum(d => d.Amount ?? 0m).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).AppendLine();

        return File(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv.ToString()),
            "text/csv; charset=utf-8", "peniel-place-donation-report.csv");
    }

    private static IReadOnlyList<ReportRow> BuildRows ( IReadOnlyList<DonationFact> facts, string groupBy )
    {
        if ( groupBy == "month" )
        {
            return facts.GroupBy(d => new DateTime(d.Date.Year, d.Date.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new ReportRow(g.Key.ToString("MMM yyyy"), g.Count(), g.Sum(d => d.Amount ?? 0m)))
                .ToList();
        }

        if ( groupBy == "donor" )
        {
            return facts.GroupBy(d => d.IsAnonymous || d.DonorId is null ? "Anonymous" : d.DonorName)
                .Select(g => new ReportRow(g.Key, g.Count(), g.Sum(d => d.Amount ?? 0m)))
                .OrderByDescending(g => g.Total).ThenBy(g => g.Label, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return facts.GroupBy(d => d.CategoryName)
            .Select(g => new ReportRow(g.Key, g.Count(), g.Sum(d => d.Amount ?? 0m)))
            .OrderByDescending(g => g.Total).ThenBy(g => g.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ValidRange ( DateTime from, DateTime to ) =>
        from.Date >= new DateTime(2000, 1, 1) && to.Date <= DateTime.Today && from.Date <= to.Date;

    private static bool IsAllowedGroup ( string groupBy ) => groupBy is "category" or "donor" or "month";

    private static string CsvCell ( string? value )
    {
        var text = value ?? string.Empty;
        // Protect spreadsheet users from formula injection if a label ever starts with a formula marker.
        var first = text.TrimStart();
        if ( first.StartsWith('=') || first.StartsWith('+') || first.StartsWith('-') || first.StartsWith('@') )
            text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}