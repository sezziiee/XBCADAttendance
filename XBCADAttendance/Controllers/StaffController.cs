using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult CreateLecturer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateLecturer(AddStaffViewModel model)
        {
            var viewModel = new AddStaffViewModel();
            
            
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Users(AdminViewModel model) 
        {
            return View(model);
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
