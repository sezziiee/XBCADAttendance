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
        //private readonly DbWilContext _dbContext;
        //public StaffController(DbWilContext dbContext)
        //{
        //    _dbContext = dbContext;
        //}
        //[Authorize(Policy = "LecturerOnly")]
        public IActionResult Index()
        {
            return View();
        }
        // Add Admin/Lecturer Policy?
        public IActionResult LectureReport(LectureReportViewModel model)
        {
            return View(model);
        }

        //[Authorize(Policy = "LecturerOnly")]
        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

       
        [HttpGet]
       // [Authorize(Policy = "LecturerOnly")]
        public IActionResult Create()
        {
            CreateLectureViewModel model = new CreateLectureViewModel(User.Identity.Name);
            return View(model);
        }

        [HttpPost]
       // [Authorize(Policy = "LecturerOnly")]
        public IActionResult Create(TblStaffLecture lecture)
        {
            lecture.LectureId = "L" + DataAccess.GetAllStaffLectures().Count().ToString();
            lecture.UserId = User.Identity.Name;
            DataAccess.AddLecture(lecture);

            return RedirectToAction("Index", "Staff");
        }

       // [Authorize(Policy = "LecturerOnly")]
        public IActionResult LecturerQRCode()
        {
            LectureReportViewModel newModel = new LectureReportViewModel();
            byte[] qrCodeImage = newModel.GenerateQRCode();

            return File(qrCodeImage, "image/png");
        }

       // [Authorize(Policy = "LecturerOnly")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Index", "Home");
        }

        /*NICHOLAS'S CODE FOR INLINE EDITING DO NOT TOUCH*/

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public JsonResult UpdateUser(int id, string fieldName, string value)
        //{
        //    var user = _dbContext.Users.Find(id);

        //    if (user == null)
        //    {
        //        return Json(new { success = false, message = "User not found." });
        //    }

        //    switch (fieldName)
        //    {
        //        case "UserName":
        //            user.UserName = value;
        //            break;
        //        case "Password":
        //            user.Password = value; // Ensure you hash the password properly in a real scenario
        //            break;
        //        case "Role":
        //            user.Role = value;
        //            break;
        //        default:
        //            return Json(new { success = false, message = "Invalid field." });
        //    }

        //    _dbContext.SaveChanges();
        //    return Json(new { success = true });
        //}
        //[HttpPost]
        //public IActionResult SaveChanges([FromBody] Dictionary<string, Dictionary<string, string>> editedData)
        //{
        //    try
        //    {
        //        // Iterate through each modified user
        //        foreach (var userId in editedData.Keys)
        //        {
        //            var user = _context.Users.Find(Convert.ToInt32(userId));
        //            if (user == null) continue;

        //            // Update each modified field
        //            foreach (var field in editedData[userId].Keys)
        //            {
        //                switch (field)
        //                {
        //                    case "UserName":
        //                        user.UserName = editedData[userId][field];
        //                        break;
        //                    case "Password":
        //                        user.Password = editedData[userId][field];
        //                        break;
        //                    case "Role":
        //                        user.Role = editedData[userId][field];
        //                        break;
        //                }
        //            }
        //            _context.SaveChanges();
        //        }
        //        return Json(new { success = true });
        //    }
        //    catch
        //    {
        //        return Json(new { success = false });
        //    }
        //}


    }
}
