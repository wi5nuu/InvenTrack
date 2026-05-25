using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
