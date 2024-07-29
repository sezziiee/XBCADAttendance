using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StudentLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult StudentLogin(LoginViewModel model) 
        {
            string message = DataAccess.GetContext().LoginUser();

            ViewBag.Message = message;

            return View(model);
        }
    }
}
