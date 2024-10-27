using Microsoft.AspNetCore.Mvc;
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
    }
}
