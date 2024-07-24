using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;

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
            string message = DataAccess.GetContext().AddStudent(model.UserID, model.StudentNo, model.Username, model.Password);

            ViewBag.Message = message;

            return View(model);
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult AddStaff(AddStaffViewModel model)
        {
            return View();
        }


    }
}
