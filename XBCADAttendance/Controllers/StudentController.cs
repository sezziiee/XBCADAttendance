using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    [Authorize(Policy = "StudentOnly")]
    public class StudentController : Controller
    {
        [Authorize(Policy = "StudentOnly")]
        public IActionResult Index()
        {
            
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            string? userID = User.Identity.Name;
            var user = DataAccess.GetUserById(userID);
            string userPassword = user.Result.Password;

            if (string.IsNullOrWhiteSpace(userID))
            {
                // Optionally add an error message to display on the redirected page
                TempData["ErrorMessage"] = "User ID is invalid. Please log in again.";
                return RedirectToAction("Index", "Home");
            }
            
            Hasher passwordHasher = new Hasher("0000"); 
            string hashed = passwordHasher.GetHash();
            ViewData["UserPassword"] = user.Result.Password; 
            ViewData["hashed"] = hashed;

            StudentReportViewModel model = new StudentReportViewModel(userID);
            return View(model);
        }

        [Authorize(Policy = "StudentOnly")]
        public IActionResult Report(string? moduleCode, DateOnly? start, DateOnly? end, string? status)
        {
            string? userID = null;

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    StudentReportViewModel model = new StudentReportViewModel(userID);
                    model.ApplyFilters(moduleCode, start, end, status);
                    return View(model);
                } else
                {
                     return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }


        [Authorize(Policy = "StudentOnly")]
        public IActionResult Profile()
        {
            string? userID = null;

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);
                    return View(newModel);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");

        }

        [Authorize(Policy = "StudentOnly")]
        public IActionResult Modules()
        {
            string? userID = null;

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);
                    return View(newModel);
                } else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "StudentOnly")]
        public IActionResult AttendanceHistory()
        {
            string? userID = null;

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);
                    return View(newModel);
                } else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "StudentOnly")]
        public IActionResult StudentQRCode()
        {
            if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
            {
                var userID = User.Identity.Name;
                StudentReportViewModel newModel = new StudentReportViewModel(userID);
                byte[] qrCodeImage = newModel.GenerateQRCode();

                return File(qrCodeImage, "image/png"); // Returns the QR code image directly
            }

            return RedirectToAction("Index", "Student");
        }

        [Authorize(Policy = "StudentOnly")]
        [HttpPost]
        public async Task<IActionResult> Update(string username, string password)
        {
            string? userID = null;
            Hasher passwordHasher = new Hasher(password!);
            string userPassword = passwordHasher.GetHash();

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!string.IsNullOrEmpty(userID))
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);                  
                  await newModel.UpdateUserCredentialsAsync(userID, username, userPassword);
                    ViewData["Success"] = "True";
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }


        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Index", "Home");
        }




    }
}
