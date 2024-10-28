using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Permissions;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        public IActionResult UserReport(AdminViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult LectureReport(AdminViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Name()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View(new UserInfo(User.Identity.Name));
            }

            return RedirectToAction();
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Number()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View(new UserInfo(User.Identity.Name));
            }

            return RedirectToAction();
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Password()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View(new UserInfo(User.Identity.Name));
            }

            return RedirectToAction();
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Role()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View(new UserInfo(User.Identity.Name));
            }

            return RedirectToAction();
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Name(UserInfo userInfo)
        {
            try
            {
                var user = DataAccess.GetUserById(userInfo.userId);

                if (user != null)
                {
                    user.UserName = userInfo.name;
                    DataAccess.context.TblUsers.Update(user);

                    DataAccess.context.SaveChanges();

                    RedirectToAction("UserReport","Admin");
                } else throw new Exception("No User");
            
                return View(userInfo);
            } catch (Exception ex)
            {
                return View(userInfo);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Number(UserInfo userInfo)
        {
            try
            {
                var user = DataAccess.GetUserById(userInfo.userId);

                if (user != null)
                {
                    if (user.TblStudent != null)
                    {
                        var student = user.TblStudent;

                        student.StudentNo = userInfo.identifier;
                        DataAccess.context.TblStudents.Update(student);

                        DataAccess.context.SaveChanges();

                        RedirectToAction("UserReport", "Admin");
                    }else
                    {
                        var staff = user.TblStaff;

                        staff.StaffId = userInfo.identifier;
                        DataAccess.context.TblStaffs.Update(staff);

                        DataAccess.context.SaveChanges();

                        RedirectToAction("UserReport", "Admin");
                    }
                    
                } else throw new Exception("No User");

                return View(userInfo);
            } catch (Exception ex)
            {
                return View(userInfo);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Password(UserInfo userInfo)
        {
            try
            {
                var user = DataAccess.GetUserById(userInfo.userId);

                if (user != null)
                {
                    Hasher hasher = new Hasher(userInfo.password);
                    user.Password = hasher.GetHash();
                    DataAccess.context.TblUsers.Update(user);

                    DataAccess.context.SaveChanges();

                    RedirectToAction("UserReport", "Admin");
                } else throw new Exception("No User");

                return View(userInfo);
            } catch (Exception ex)
            {
                return View(userInfo);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Edit_Role(UserInfo userInfo)
        {
            try
            {
                var user = DataAccess.GetUserById(userInfo.userId);

                if (user != null)
                {
                    if (user.TblStaff != null)
                    {
                        var staff = user.TblStaff;

                        userInfo.UpdateRoleId();

                        staff.RoleId = userInfo.roleId;
                        DataAccess.context.TblStaffs.Update(staff);

                        DataAccess.context.SaveChanges();

                        RedirectToAction("UserReport", "Admin");
                    }
                } else throw new Exception("No User");

                return View(userInfo);
            } catch (Exception ex)
            {
                return View(userInfo);
            }
        }
    }

    public struct UserInfo
    {
        public string userId { get; set; }
        public string identifier { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public List<string> roles = DataAccess.GetAllRoles().Select(x => x.RoleName).ToList();
        public string roleId { get; set; }
        public string role { get; set; }

        public UserInfo(string userId)
        {
            this.userId = userId;

            var user = DataAccess.context.TblUsers.Where(x => x.UserId == userId).FirstOrDefault();

            if (user != null)
            {
                if (user.TblStudent != null)
                {
                    identifier = user.TblStudent.StudentNo.ToString();
                    role = "Student";
                }else 
                { 
                    identifier = user.TblStaff.StaffId.ToString();
                }

                name = user.UserName;

            }
        }
        [Authorize(Policy = "AdminOnly")]
        public void UpdateRoleId()
        {
            roleId = roles.IndexOf(role).ToString();
        }
    }
}
