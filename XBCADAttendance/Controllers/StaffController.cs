using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            CreateLectureViewModel model = new CreateLectureViewModel(User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Create(TblStaffLecture lecture)
        {
            lecture.LectureId = "L" + DataAccess.GetAllStaffLectures().Count().ToString();
            lecture.UserId = User.Identity.Name;
            DataAccess.AddLecture(lecture);

            return RedirectToAction("Index", "Staff");
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
