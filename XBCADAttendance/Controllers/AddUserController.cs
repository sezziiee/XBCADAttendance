using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class AddUserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStudent(AddStudentViewModel model)
        {
            string message = DataAccess.AddStudent(model.UserID, model.StudentNo, model.Username, model.Password);

            ViewBag.Message = message;

            return View(model);
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            return View(new AddStaffViewModel());
        }
        
        [HttpPost]
        public IActionResult AddStaff(AddStaffViewModel model)
        {
            try
            {
                TblUser user = new TblUser
                {
                    UserName = model.Name,
                    UserId = model.UserId,
                    Password = model.Password
                };

                TblStaff staff = new TblStaff 
                { 
                    UserId = model.UserId,
                    RoleId = model.RoleID,
                    StaffId = model.StaffNumber,
                };

                DataAccess.context.TblUsers.Add(user);
                DataAccess.context.TblStaffs.Add(staff);
                DataAccess.context.SaveChanges();

            } catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(model);
            }

            return RedirectToAction("Index","LectureReport");
        }
    }
}
