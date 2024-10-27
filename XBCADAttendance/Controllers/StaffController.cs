using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LectureReport(LectureReportViewModel model)
        {
            return View(model);
        }

        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
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

        [HttpPost]
        public IActionResult Create(AddStaffViewModel model)
        {
            var viewModel = new AddStaffViewModel(); 
            return View(viewModel);
        }

        [HttpPost]
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Create(TblStaffLecture lecture)
        {
            lecture.LectureId = "L" + DataAccess.GetAllLectures().Count().ToString();
            lecture.UserId = User.Identity.Name;
            DataAccess.AddLecture(lecture);

            return View();
        }

        public IActionResult LecturerQRCode()
        {
            LectureReportViewModel newModel = new LectureReportViewModel();
            byte[] qrCodeImage = newModel.GenerateQRCode();

            return File(qrCodeImage, "image/png");
        }
        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Index", "Home");
        }
    }
}
