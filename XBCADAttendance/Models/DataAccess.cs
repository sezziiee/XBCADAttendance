using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using XBCADAttendance.Models.ViewModels;
using XBCADAttendance;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Azure.Core;

namespace XBCADAttendance.Models
{
    public class DataAccess
    {
        public static DbWilContext context = new DbWilContext();
        public static DataAccess? instance = null;

        public DataAccess() {
            GetInstance();
        }

        public static DataAccess GetInstance()
        {
            if (instance == null)
            {
                instance = new DataAccess();
            }

            return instance;
        }

        [ValidateAntiForgeryToken]
        public static string LoginStudent(HttpContext httpContext, LoginViewModel model)
        {
            if (model.identifier.Length != 8 || model.identifier.Length != 10)//Check if id is for user/staff
            {
                var student = context.TblStudents.Where(x => x.StudentNo == model.identifier).FirstOrDefault();
                
                if (student != null)
                {
                    var user = context.TblUsers.Where(x => x.UserId == student.UserId).FirstOrDefault();

                    if (user != null)
                    {
                        Hasher passwordHasher = new Hasher(model.password!);
                        string userPassword = passwordHasher.GetHash();

                        if (passwordHasher.CompareHashedPasswords(userPassword, user.Password))
                        {//Login logic
                            StoreUserCookies(httpContext, student.UserId, "Student");

                            return "Success";
                        } else return "Incorrect password";

                    } else return "Student not found";
                }
                return "Student not found";
            }
            return "Invalid student number";
        }

        [ValidateAntiForgeryToken]
        public static string LoginStaff(HttpContext httpContext, LoginViewModel model)
        {
            
            if (model.identifier.Length == 5)
            {
                var staff = context.TblStaffs.Where(x => x.StaffId == model.identifier).FirstOrDefault();
                var user = context.TblUsers.Where(x => x.UserId == staff.UserId).FirstOrDefault();

                if (user != null)
                {
                    Hasher passwordHasher = new Hasher(model.password!);

                    string userPassword = passwordHasher.GetHash();

                    if (passwordHasher.CompareHashedPasswords(userPassword, user.Password))
                    {//Add Login logic later
                        var role = GetAllRoles().Where(x => x.RoleId == staff.RoleId).Select(x => x.RoleName).ToString();
                        StoreUserCookies(httpContext, staff.UserId, role);
                        return "Success";
                    } else return "Incorrect password";
                } else
                {
                    return "Staff not found";
                }
            }

            return "Invalid staff number";
        }

        //Sign in and authentication
        // Stores user authentication cookies using ASP.NET Core's cookie authentication.
        public static async Task StoreUserCookies(HttpContext httpContext, string id, string role)
        {
            // Create a list of claims for the user with the student's number as the user's name.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, id),
                new Claim(ClaimTypes.Role, role)
            };

            // Create a ClaimsIdentity using the claims list and the default authentication scheme for cookies.
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Define authentication properties, making the session persistent (e.g., "Remember Me").
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            // Sign in the user asynchronously, storing their identity and properties in a cookie.
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,  // Authentication scheme used.
                new ClaimsPrincipal(claimsIdentity),  // The user's identity information.
                authProperties  // Properties for session persistence.
            );
        }

        public static int CalcDaysAttended(string studentNo)
        {
            List<TblStudentLecture>? studentLectures = GetAllLecturesByStudentNo(studentNo);
            int count = 0;

            foreach(TblStudentLecture lecture in studentLectures)
            {
                if(lecture.ScanOut != null)
                {
                    count++;
                }
            }
            
            return count;
        }

        public static async Task Logout(HttpContext httpContext)
        {
            // Removing the users cookies
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        //CRUD Operations

        //Create
        //
        public static string AddStudent(string userID, string studentNo, string userName, string passWord)
        {
            // userID = "10085210";
            if (userID != null && studentNo != null && userName != null && passWord != null)
            {
                Hasher hasher = new Hasher(passWord);

                try
                {
                    TblUser newUser = new TblUser
                    {
                        UserId = userID,
                        UserName = userName,
                        Password = hasher.GetHash()
                    };

                    TblStudent newStudent = new TblStudent
                    {
                        StudentNo = studentNo,
                        UserId = userID,
                    };

                    context.TblUsers.Add(newUser);
                    context.TblStudents.Add(newStudent);
                    context.SaveChanges();

                    return "Success";

                }
                catch (Exception e)
                {
                    return e.ToString();
                }
            }
            else return "Please fill in all fields";
        }

        //Read
        public static TblUser? GetUserById(string userID)
        {
            return context.TblUsers.Where(x => x.UserId == userID).FirstOrDefault();
        }

        public static List<string>? GetModulesById(string userID)
        {
            var user = GetUserById(userID);

            if (user != null)
            {
                if (user.TblStaff != null)
                {
                    return context.TblStaffLectures.Select(x => x.ModuleCode).Distinct().ToList();
                } else
                {
                    return context.TblStudentLectures.Select(x => x.ModuleCode).Distinct().ToList();
                }
            }

            return null;
        }

        public static TblUser? GetUserByStudentNo(string studentNo)
        {
            return context.TblUsers.Where(x => x.TblStudent.StudentNo == studentNo).FirstOrDefault();
        }

        public static List<TblStudent> GetAllStudents()
        {
            var data = context.TblStudents.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public static List<TblStudentLecture>? GetStudentLectures(string userID)
        {
            var lectures = context.TblStudentLectures.Where(x => x.UserId == userID).ToList();

            return lectures;
        }

        public static float GetStudentAttendance(string studentNo)
        {
            var student = context.TblStudents.Where(x => x.StudentNo == studentNo).FirstOrDefault();
            var totalLectures = GetStudentLectures(student.UserId);
            var attendance = context.TblStudentLectures.Where(x => x.UserId == student.UserId && x.ScanOut != null).ToList();

            if (totalLectures.Count > 0)
            {
                return ((float)attendance.Count / totalLectures.Count) * 100;
            }

            return 0;
            
        }

        public static string? GetIdByStudentNo(string studentNo) 
        { 
            var students = context.TblUsers.Where(x => x.TblStudent != null).Select(x => x.TblStudent);

            return students.Where(x => x!.StudentNo == studentNo).Select(x => x!.UserId).FirstOrDefault();
        }

        public static string? GetStudentNoById(string userID)
        {
            return context.TblUsers.Where(x => x.UserId == userID && x.TblStudent != null).Select(x => x.TblStudent!.StudentNo).FirstOrDefault();
        }

        public static List<TblStudentLecture> GetAllLectures()
        {
            var data = context.TblStudentLectures.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public static List<TblStudentLecture>? GetAllLecturesByStudentNo(string studentNo)
        {
            var studentID = context.TblUsers.Where(x => x.TblStudent != null && x.TblStudent.StudentNo == studentNo).Select(x => x.UserId).FirstOrDefault();
            var data = context.TblStudentLectures.Where(x => x.UserId == studentID).ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static List<TblModule>? GetAllModules()
        {
            var data = context.TblModules.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public static List<TblModule> GetModulesByStudentNo(string studentNo)
        {
            string userId = GetIdByStudentNo(studentNo)!;
            var lectures = context.TblStudentLectures.Where(x => x.UserId == userId).Select(x => x.ModuleCode).ToList();
            var modules = context.TblModules.ToList();
            
            List<TblModule> lstModules = new List<TblModule>();

            foreach (var lecture in lectures) 
            {
                var studentModules = modules.Where(x => x.ModuleCode == lecture).ToList();
                lstModules.AddRange(studentModules);
            }

            return lstModules;
        }

        public static List<TblUser> GetAllUsers()
        {
            var data = context.TblUsers.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public static List<TblStaff> GetAllStaff()
        {
            var data = context.TblStaffs.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public static List<TblRole> GetAllRoles()
        {
            var data = context.TblRoles.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        //Update
        public static string UpdateUser(string userID, string? userName, string? passWord)
        {
            bool updateName = false;
            bool updatePassword = false;

            //Get User from DB using userID
            var user = context.TblUsers.Where(x => x.UserId == userID).FirstOrDefault();

            //Null check for User
            if (user != null)
            {
                //Null check for username
                if (userName != null)
                {
                    updateName = true;
                }

                //Null check for password
                if (passWord != null)
                {
                    updatePassword = true;
                }

                if (updateName && updatePassword) //Update both username and password
                {
                    user.UserName = userName;
                    user.Password = passWord;

                    context.SaveChanges();
                    return "User updated successfully";

                }
                else if (updateName && !updatePassword) //Update username
                {
                    user.Password = passWord;

                    context.SaveChanges();
                    return "Password updated successfully";

                }
                else if (!updateName && updatePassword) //Update password
                {
                    user.UserName = userName;

                    context.SaveChanges();
                    return "Username updated successfully";

                }
                else //Handle if no variables are entered
                {
                    return "No values were updated";
                }
            }
            else //Handle if userID is not found 
            {
                return "User not found";
            }

        }

        //Delete
        public static string DeleteUser(string userID)
        {
            try
            {
                var user = context.TblUsers.Where(x => x.UserId == userID).FirstOrDefault();


                if (user != null)
                {
                    //user.TblStudentLectures.Clear();

                    if (user.TblStaff != null)
                    {
                        context.TblStaffs.Where(x => x.UserId == userID).ExecuteDelete();
                        context.TblStaffLectures.Where(x => x.UserId == userID).ExecuteDelete();

                    }else if (user.TblStudent != null)
                    {
                        context.TblStudents.Where(x => x.UserId == userID).ExecuteDelete();
                        context.TblStudentLectures.Where(x => x.UserId == userID).ExecuteDelete();
                    }
                }

                context.TblUsers.Where(x => x.UserId == userID).ExecuteDelete();

                context.SaveChanges();
                return "User deleted successfully";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }

        }

        public static string DeleteLecture(string lectureID)
        {
            try
            {
                context.TblStudentLectures.Where(x => x.LectureId == lectureID).ExecuteDelete();
                context.SaveChanges();
                return "Lecture deleted successfully";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static string DeleteModule(string moduleCode)
        {
            try
            {
                context.TblModules.Where(x => x.ModuleCode == moduleCode).ExecuteDelete();
                context.SaveChanges();
                return "Module deleted successfully";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static void AddLecture(TblStaffLecture lecture)
        {
            //Add error handling
            if (lecture != null)
            {
                context.TblStaffLectures.Add(lecture);
            }

            context.SaveChanges();
        }

        public static List<TblStaffLecture> GetStaffLectures()
        {
            return context.TblStaffLectures.ToList();
        }

        public static List<TblStudent?> GetStudentsByModule(string moduleCode)
        {
            return context.TblStudentLectures.Where(x => x.ModuleCode == moduleCode).Select(x => GetStudentById(x.UserId)).ToList();
        }

        public static TblStudent? GetStudentById(string userId)
        {
            return context.TblStudents.Where(x => x.UserId == userId).FirstOrDefault();
        }

        public static List<TblStudentLecture>? GetStudentLecturesByStaffId(string staffId)
        {
            var user = context.TblStaffs.Where(x => x.StaffId == staffId).FirstOrDefault();

            if (user != null)
            {
                return context.TblStudentLectures.Where(x => x.UserId == user.UserId).ToList();
            }
            
            return null;
        }
    }
}
