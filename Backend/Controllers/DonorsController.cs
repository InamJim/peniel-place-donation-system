using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PenielPlaceMinistriesCSharp.Models;
using PenielPlaceMinistriesCSharp.Services;

namespace PenielPlaceMinistriesCSharp.Controllers;

[Authorize]
public class DonorsController(IDonorService donors) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(await donors.AllAsync());

    [HttpGet]
    public IActionResult Create() =>
        View(new DonorRequest());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DonorRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        try
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var donor = await donors.CreateAsync(request, userId);

            TempData["Success"] = "Donor created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = donor.Id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var donor = await donors.FindAsync(id);

        return donor is null ? NotFound() : View(donor);
    }
}