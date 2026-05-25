using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Dashboard");
    }

    public IActionResult Error(string? message = null)
    {
        ViewBag.ErrorMessage = message ?? "An unexpected error occurred.";
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
