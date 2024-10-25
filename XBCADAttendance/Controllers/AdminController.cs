using Microsoft.AspNetCore.Mvc;

namespace XBCADAttendance.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
