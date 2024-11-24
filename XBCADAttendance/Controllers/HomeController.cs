using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            DataAccess.GetInstance();
            return View();
            
        }
        [HttpPost]
        public async Task<IActionResult> StudentLogin(LoginViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid login data." });
                }

                if (string.IsNullOrEmpty(model.identifier))
                {
                    return Json(new { success = false, message = "Please enter your student number." });
                }

                if (string.IsNullOrEmpty(model.password))
                {
                    return Json(new { success = false, message = "Please enter a password." });
                }

                if (model.identifier.Length < 10 && !model.identifier.ToLower().StartsWith("st"))
                {
                    model.identifier = "ST" + model.identifier;
                }

                string? message = await DataAccess.LoginStudent(HttpContext, model);

                if (message == "Success")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Student", new { userID = model.identifier }) });
                }

                return Json(new { success = false, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.identifier))
                {
                    return Json(new { success = false, message = "Please enter your staff number." });
                }

                if (string.IsNullOrEmpty(model.password))
                {
                    return Json(new { success = false, message = "Please enter a password." });
                }

                string? message = await DataAccess.LoginStaff(HttpContext, model);

                if (message == "Administrator")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });
                }
                else if (message == "Lecturer")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Staff") });
                }
                else
                {
                    return Json(new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
