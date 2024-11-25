using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;
using Newtonsoft.Json;

namespace XBCADAttendance.Controllers
{
    public class LoginController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult StudentLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StudentLogin(LoginViewModel model)
        {
            try
            {
                if (model == null)
                {
                    ViewBag.Message = "Invalid login data.";
                    return View();
                }


                if (string.IsNullOrEmpty(model.identifier))
                {
                    ViewBag.Message = "Please enter your student/Staff number.";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.password))
                {
                    ViewBag.Message = "Please enter a password.";
                    return View(model);
                }

                if (model.identifier.Length < 10)
                {
                    if (!model.identifier.ToLower().StartsWith("st"))
                    {
                        model.identifier = "ST" + model.identifier;
                    }
                    else
                    {
                        ViewBag.Message = "Please enter a valid student number.";
                        return View(model);
                    }
                }

                string? message = await DataAccess.LoginStudent(HttpContext, model);

                ViewBag.Message = message;

                if (message == "Success")
                {
                    return RedirectToAction("Index", "Student", new { userID = model.identifier });
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ViewBag.Message = "An error occurred while processing your request. Please try again later.";
                return View(model);
            }
        }



        public IActionResult StaffLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.identifier))
            {
                ViewBag.Message = "Please enter your lecturer number.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.password))
            {
                ViewBag.Message = "Please enter a password.";
                return View(model);
            }

            string? message = await DataAccess.LoginStaff(HttpContext, model);

            ViewBag.Message = message;

            if (message == "Administrator")
            {
                return RedirectToAction("Index", "Admin");
            }
            if (message == "Lecturer")
            {
                return RedirectToAction("Index", "Staff");
            }
            else
            {
                return View(model);
            }

        }
    }
}

