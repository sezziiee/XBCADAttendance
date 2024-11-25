using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.IdentityModel.Tokens;

namespace XBCADAttendance.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            LectureReportViewModel model = new LectureReportViewModel(User.Identity.Name);
            return View(model);
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
                Hasher hasher = new Hasher(model.Password);

                TblUser user = new TblUser
                {
                    UserName = model.Name,
                    UserId = model.UserId,
                    Password = hasher.GetHash()
                };

                TblStaff staff = new TblStaff
                {
                    UserId = model.UserId,
                    RoleId = model.RoleID,
                    StaffId = model.StaffNumber,
                };

                DataAccess.Context.TblUsers.Add(user);
                DataAccess.Context.TblStaffs.Add(staff);
                DataAccess.Context.SaveChanges();

            } catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(model);
            }

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UserReport()
        {

            var model = new AdminViewModel
            {
                Users = DataAccess.Context.TblUsers.ToList(),
                Students = DataAccess.Context.TblStudents.ToList(),
                Staff = DataAccess.Context.TblStaffs.ToList(),
                StaffLectures = DataAccess.Context.TblStaffLectures.ToList(),
                lstRoles = DataAccess.Context.TblRoles.ToList()
            };

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
        public async Task<IActionResult> Edit_Name(UserInfo userInfo)

        {
            try
            {
                var user = await DataAccess.GetUserById(userInfo.userId);

                if (user != null)
                {
                    user.UserName = userInfo.name;
                    DataAccess.Context.TblUsers.Update(user);

                    DataAccess.Context.SaveChanges();

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
        public IActionResult Edit_Number(UserInfo userInfo)
        {
            try
            {
                var user = DataAccess.GetUserById(userInfo.userId).Result;

                if (user != null)
                {
                    if (user.TblStudent != null)
                    {
                        var student = user.TblStudent;

                        student.StudentNo = userInfo.identifier;
                        DataAccess.Context.TblStudents.Update(student);

                        DataAccess.Context.SaveChanges();

                        RedirectToAction("UserReport", "Admin");
                    } else
                    {
                        var staff = user.TblStaff;

                        staff.StaffId = userInfo.identifier;
                        DataAccess.Context.TblStaffs.Update(staff);

                        DataAccess.Context.SaveChanges();

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
                var user = DataAccess.GetUserById(userInfo.userId).Result;

                if (user != null)
                {
                    Hasher hasher = new Hasher(userInfo.password);
                    user.Password = hasher.GetHash();
                    DataAccess.Context.TblUsers.Update(user);

                    DataAccess.Context.SaveChanges();

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
                var user = DataAccess.GetUserById(userInfo.userId).Result;

                if (user != null)
                {
                    if (user.TblStaff != null)
                    {
                        var staff = user.TblStaff;

                        userInfo.UpdateRoleId();

                        staff.RoleId = userInfo.roleId;
                        DataAccess.Context.TblStaffs.Update(staff);

                        DataAccess.Context.SaveChanges();

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
        public async Task<IActionResult> UpdateUsers(string Id, string Username, string Password)
        {
            Hasher passwordHasher = new Hasher(Password!);
            string userPassword = passwordHasher.GetHash();
            await DataAccess.UpdateUser(Id, Username, userPassword);
            return RedirectToAction("UserReport", "Admin");
            
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Profile()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    AdminViewModel newModel = new AdminViewModel();
                    return View(newModel);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

    }


    public struct UserInfo
    {
        public string userId { get; set; }
        public string identifier { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public List<string> roles = DataAccess.GetAllRoles().Result.Select(x => x.RoleName).ToList();
        public string roleId { get; set; }
        public string role { get; set; }

        public UserInfo(string userId)
        {
            this.userId = userId;

            var user = DataAccess.Context.TblUsers.Where(x => x.UserId == userId).FirstOrDefault();

            if (user != null)
            {
                if (user.TblStudent != null)
                {
                    identifier = user.TblStudent.StudentNo.ToString();
                    role = "Student";
                }
                else
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
