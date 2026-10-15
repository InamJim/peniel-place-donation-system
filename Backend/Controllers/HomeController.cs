
using Microsoft.AspNetCore.Mvc;
using PenielPlaceMinistriesCSharp.Models;
using System.Diagnostics;

namespace PenielPlaceMinistriesCSharp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            };

            return View(model);
        }
    }
}
