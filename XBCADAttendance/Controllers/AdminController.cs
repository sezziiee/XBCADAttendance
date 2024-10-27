using Microsoft.AspNetCore.Mvc;
using System.Security.Permissions;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
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

            return RedirectToAction("Index", "LectureReport");
        }

        [HttpGet]
        public IActionResult UserReport(AdminViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public IActionResult LectureReport(AdminViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit_Name()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit_Number()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit_Password()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit_Role()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Edit_Name(UserInfo userInfo)
        {
            return View(userInfo);
        }

        [HttpPost]
        public IActionResult Edit_Number(UserInfo userInfo)
        {
            return View(userInfo);
        }

        [HttpPost]
        public IActionResult Edit_Password(UserInfo userInfo)
        {
            return View(userInfo);
        }

        [HttpPost]
        public IActionResult Edit_Role(UserInfo userInfo)
        {
            return View(userInfo);
        }
    }

    public struct UserInfo 
    {   
        TblUser user { get; set; }
        public string identifier { get; set; }
        public int roleId { get; set; }
    }   
}
