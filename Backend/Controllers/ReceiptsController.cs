using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PenielPlaceMinistriesCSharp.Services;

namespace PenielPlaceMinistriesCSharp.Controllers;

[Authorize]
public class ReceiptsController(IReceiptService receipts) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Show(int id)
    {
        var donation = await receipts.GetForDonationAsync(id);

        if (donation?.Receipt is null)
            return NotFound("Receipt not found.");

        return View(donation);
    }
}