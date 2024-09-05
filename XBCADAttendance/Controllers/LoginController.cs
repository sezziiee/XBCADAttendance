using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

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
            string message = DataAccess.LoginUser(HttpContext, model);

            ViewBag.Message = message;

            //return View(model);
            return RedirectToAction("StudentReport");
            //return RedirectToAction("StudentReport", new { userID = model. });
        }
    }
}
