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
        public IActionResult StudentLogin(LoginViewModel model)
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
                    ViewBag.Message = "Please enter your student number.";
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

                string? message = DataAccess.LoginUser(HttpContext, model);

                ViewBag.Message = message;

                if (message == "Success")
                {
                    return RedirectToAction("Index", "StudentReport", new { userID = model.identifier });
                }
                else
                {
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                ViewBag.Message = "An error occurred while processing your request. Please try again later.";
                return View(model);
            }
        }



        public IActionResult StaffLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult StaffLogin(LoginViewModel model)
        {
            string? message = DataAccess.LoginUser(HttpContext, model).ToString();

            ViewBag.Message = message;

            return RedirectToAction("LectureReport", "LectureReport");
        }
    }
}
