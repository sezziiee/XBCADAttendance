using Microsoft.AspNetCore.Authorization;
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
            LectureReportViewModel model = new LectureReportViewModel(User.Identity.Name);
            return View(model);
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult LectureReport(LectureReportViewModel model)
        {
            return View(model);
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Create()
        {
            CreateLectureViewModel model = new CreateLectureViewModel(User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "LecturerOnly")]
        public async Task<IActionResult> Create(TblStaffLecture lecture)
        {
            try
            {
                if (lecture.ModuleCode.IsNullOrEmpty())
                {
                    ViewBag.Message = "Please select a Module Name.";
                    return View(lecture);
                }
                lecture.LectureId = "L" + (await DataAccess.GetAllStaffLectures()).Count().ToString();
                lecture.UserId = User.Identity.Name;
               
                await DataAccess.AddLecture(lecture);

                string? message = "Lecture created successfully.";
                ViewBag.Message = message;

                return View(new CreateLectureViewModel(User.Identity.Name));
            }
            catch(Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(lecture);
            }
        }


        [Authorize(Policy = "LecturerOnly")]
        public IActionResult LecturerQRCode()
        {
            string? userID = null;

            if (User.Identity.IsAuthenticated)
            {
                userID = User.Identity.Name;

                if (!string.IsNullOrEmpty(userID))
                {
                    LectureReportViewModel newModel = new LectureReportViewModel(userID);
                    byte[] qrCodeImage = newModel.GenerateQRCode();
                    string base64Image = Convert.ToBase64String(qrCodeImage);

                    ViewBag.QRCodeImage = base64Image;
                    return View(newModel);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Profile()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    LectureReportViewModel newModel = new LectureReportViewModel(userID);
                    return View(newModel);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AddModule()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddModule(TblModule module)
        {
            try
            {
                if (string.IsNullOrEmpty(module.ModuleName))
                {
                    ViewBag.Message = "Please enter a Module Name.";
                    return View(module);
                }
                if (string.IsNullOrEmpty(module.ModuleCode))
                {
                    ViewBag.Message = "Please enter a Module Code.";
                    return View(module);
                }

                DataAccess.Context.TblModules.Add(module);

                var existingModule = DataAccess.Context.TblModules.FirstOrDefault(m => m.ModuleCode == module.ModuleCode);
                if (existingModule != null)
                {
                    string? ErrorMessage = "Module already exists.";
                    ViewBag.Message = ErrorMessage;
                    ModelState.Clear();
                    return View(new TblModule());
                }
                DataAccess.Context.SaveChanges();

                string? message = "Module added successfully.";
                ViewBag.Message = message;

                ModelState.Clear();

                return View(new TblModule());
               
            } catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(module);
            }
        }
    }
}
