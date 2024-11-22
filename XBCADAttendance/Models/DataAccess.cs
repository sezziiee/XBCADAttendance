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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace XBCADAttendance.Models
{
    public class DataAccess
    {
        private static DbWilContext? context = null;
        private static DataAccess? instance = null;
        private static readonly object lockObj = new object();

        private DataAccess()
        {
        }

        public static DataAccess GetInstance()
        {
            // Double-checked locking for thread safety
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new DataAccess();
                        context = new DbWilContext();
                    }
                }
            }
            return instance;
        }

        public static DbWilContext Context => context;

        [ValidateAntiForgeryToken]
        public static async Task<string> LoginStudent(HttpContext httpContext, LoginViewModel model)
        {
            if (model.identifier.Length == 10) // Check if ID length is valid
            {
                var student = await context.TblStudents
                    .Where(x => x.StudentNo == model.identifier)
                    .FirstOrDefaultAsync();

                if (student != null)
                {
                    var user = await context.TblUsers
                        .Where(x => x.UserId == student.UserId)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        Hasher passwordHasher = new Hasher(model.password!);
                        string userPassword = passwordHasher.GetHash();

                        if (passwordHasher.CompareHashedPasswords(userPassword, user.Password))
                        {
                            // Login logic
                            await StoreUserCookies(httpContext, student.UserId, "Student");
                            return "Success";
                        } else return "Incorrect password";
                    } else return "Student not found";
                }
                return "Student not found";
            }
            return "Invalid student number";
        }

        [ValidateAntiForgeryToken]
        public static async Task<string> LoginStaff(HttpContext httpContext, LoginViewModel model)
        {
            if (model.identifier.Length == 5)
            {
                var staff = await context.TblStaffs
                    .Where(x => x.StaffId == model.identifier)
                    .FirstOrDefaultAsync();

                if (staff != null)
                {
                    var user = await context.TblUsers
                        .Where(x => x.UserId == staff.UserId)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        Hasher passwordHasher = new Hasher(model.password!);
                        string userPassword = passwordHasher.GetHash();

                        if (passwordHasher.CompareHashedPasswords(userPassword, user.Password))
                        {
                            // Fetch role asynchronously
                            var role = GetAllRoles().Result
                                .Where(x => x.RoleId == staff.RoleId)
                                .Select(x => x.RoleName)
                                .FirstOrDefault();

                            await StoreUserCookies(httpContext, staff.UserId, role ?? "Unknown");
                            return role;
                        } else
                        {
                            return "Incorrect password";
                        }
                    } else
                    {
                        return "Staff not found";
                    }
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

        public static async Task<int> CalcDaysAttended(string studentNo)
        {
            // Assuming GetAllLecturesByStudentNo is an async method
            List<TblStudentLecture> studentLectures = await GetAllLecturesByStudentNo(studentNo);
            int count = 0;

            foreach (TblStudentLecture lecture in studentLectures)
            {
                if (lecture.ScanOut != null)
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
        public static async Task<string> AddStudent(string userID, string studentNo, string userName, string passWord)
        {
            if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(studentNo) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(passWord))
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

                    await context.TblUsers.AddAsync(newUser); // Use AddAsync for adding the new user
                    await context.TblStudents.AddAsync(newStudent); // Use AddAsync for adding the new student
                    await context.SaveChangesAsync(); // Use SaveChangesAsync for saving changes

                    return "Success";
                } catch (Exception e)
                {
                    return e.ToString();
                }
            } else
            {
                return "Please fill in all fields";
            }
        }

        //Read
        public static async Task<TblUser?> GetUserById(string userID)
        {
            return await context.TblUsers.Where(x => x.UserId == userID).FirstOrDefaultAsync();
        }

        public static async Task<List<string>?> GetModulesById(string userID)
        {
            var user = await GetUserById(userID);

            if (user != null)
            {
               
                return await context.TblUserModules
                                    .Where(x => x.UserId == userID)
                                    .Select(x => x.ModuleCode)
                                    .Distinct()
                                    .ToListAsync();
            }

            return null;
        }

        public static async Task<TblUser?> GetUserByStudentNo(string studentNo)
        {
            return await context.TblUsers.Where(x => x.TblStudent.StudentNo == studentNo).FirstOrDefaultAsync();
        }

        public static async Task<List<TblStudent>> GetAllStudents()
        {
            var data = await context.TblStudents.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblStudentLecture>?> GetStudentLectures(string userID)
        {
            var lectures = await context.TblStudentLectures.Where(x => x.UserId == userID).ToListAsync();

            return lectures;
        }

        public static async Task<float> GetStudentAttendance(string studentNo)
        {
            var student = await context.TblStudents.Where(x => x.StudentNo == studentNo).FirstOrDefaultAsync();
            var totalLectures = await GetStudentLectures(student.UserId);
            var attendance = await context.TblStudentLectures.Where(x => x.UserId == student.UserId && x.ScanOut != null).ToListAsync();

            if (totalLectures.Count > 0)
            {
                return ((float)attendance.Count / totalLectures.Count) * 100;
            }

            return 0;
        }

        public static async Task<string?> GetIdByStudentNo(string studentNo)
        {
            var students = await context.TblUsers.Where(x => x.TblStudent != null).Select(x => x.TblStudent).ToListAsync();

            return students.Where(x => x!.StudentNo == studentNo).Select(x => x!.UserId).FirstOrDefault();
        }

        public static async Task<string?> GetStudentNoById(string userID)
        {
            return await context.TblUsers.Where(x => x.UserId == userID && x.TblStudent != null).Select(x => x.TblStudent!.StudentNo).FirstOrDefaultAsync();
        }

        public static async Task<List<TblStudentLecture>> GetAllLectures()
        {
            var data = await context.TblStudentLectures.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblStaffLecture>> GetAllStaffLectures()
        {
            var data = await context.TblStaffLectures.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblStudent>> GetStudentsFromLecture(string lectureId)
        {
            List<TblStudent> output = new List<TblStudent>();

            var lectures = await context.TblStudentLectures.Where(x => x.LectureId == lectureId).ToListAsync();
            List<string> studentIds = new List<string>();

            foreach (var lecture in lectures)
            {
                studentIds.Add(lecture.UserId);
            }

            foreach (var id in studentIds)
            {
                output.Add(await context.TblStudents.Where(x => x.UserId == id).FirstOrDefaultAsync());
            }

            return output;
        }

        public static async Task<List<TblStudentLecture>?> GetAllLecturesByStudentNo(string studentNo)
        {
            var studentID = await context.TblUsers.Where(x => x.TblStudent != null && x.TblStudent.StudentNo == studentNo).Select(x => x.UserId).FirstOrDefaultAsync();
            var data = await context.TblStudentLectures.Where(x => x.UserId == studentID).ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblModule>?> GetAllModules()
        {
            var data = await context.TblModules.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblModule>> GetModulesByStudentNo(string studentNo)
        {
            string userId = await GetIdByStudentNo(studentNo)!;
            var lectures = await context.TblStudentLectures.Where(x => x.UserId == userId).Select(x => x.ModuleCode).Distinct().ToListAsync();
            var modules = await context.TblModules.ToListAsync();

            List<TblModule> lstModules = new List<TblModule>();

            foreach (var lecture in lectures)
            {
                var studentModules = modules.Where(x => x.ModuleCode == lecture).ToList();
                lstModules.AddRange(studentModules);
            }

            return lstModules;
        }

        public static async Task<List<TblUser>> GetAllUsers()
        {
            var data = await context.TblUsers.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblStaff>> GetAllStaff()
        {
            var data = await context.TblStaffs.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public static async Task<List<TblRole>> GetAllRoles()
        {
            var data = await context.TblRoles.ToListAsync();

            if (data != null)
            {
                return data;
            } else return null;
        }

        //Update
        public static async Task<string> UpdateUser(string userID, string? userName, string? passWord)
        {
            bool updateName = false;
            bool updatePassword = false;

            //Get User from DB using userID
            var user = await context.TblUsers.Where(x => x.UserId == userID).FirstOrDefaultAsync();

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

                } else if (updateName && !updatePassword) //Update username
                {
                    user.Password = passWord;

                    context.SaveChanges();
                    return "Password updated successfully";

                } else if (!updateName && updatePassword) //Update password
                {
                    user.UserName = userName;

                    context.SaveChanges();
                    return "Username updated successfully";

                } else //Handle if no variables are entered
                {
                    return "No values were updated";
                }
            } else //Handle if userID is not found 
            {
                return "User not found";
            }

        }

        //Delete
        public static async Task<string> DeleteUser(string userID)
        {
            try
            {
                var user = await context.TblUsers.Where(x => x.UserId == userID).FirstOrDefaultAsync();


                if (user != null)
                {
                    //user.TblStudentLectures.Clear();

                    if (user.TblStaff != null)
                    {
                        await context.TblStaffs.Where(x => x.UserId == userID).ExecuteDeleteAsync();
                        await context.TblStaffLectures.Where(x => x.UserId == userID).ExecuteDeleteAsync();

                    } else if (user.TblStudent != null)
                    {
                        await context.TblStudents.Where(x => x.UserId == userID).ExecuteDeleteAsync();
                        await context.TblStudentLectures.Where(x => x.UserId == userID).ExecuteDeleteAsync();
                    }
                }

                await context.TblUsers.Where(x => x.UserId == userID).ExecuteDeleteAsync();

                await context.SaveChangesAsync();
                return "User deleted successfully";

            } catch (Exception e)
            {
                return $"Error: {e}";
            }

        }

        public static async Task<string> DeleteLecture(string lectureID)
        {
            try
            {
                await context.TblStudentLectures.Where(x => x.LectureId == lectureID).ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                return "Lecture deleted successfully";

            } catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static async Task<string> DeleteModule(string moduleCode)
        {
            try
            {
                await context.TblModules.Where(x => x.ModuleCode == moduleCode).ExecuteDeleteAsync();
                await context.SaveChangesAsync();
                return "Module deleted successfully";

            } catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public static async Task AddLecture(TblStaffLecture lecture)
        {
            //Add error handling
            if (lecture != null)
            {
                await context.TblStaffLectures.AddAsync(lecture);

                var userModule = context.TblUserModules.Where(x => x.ModuleCode == lecture.ModuleCode).FirstOrDefault();

                if (userModule == null)
                {
                    context.TblUserModules.Add(new TblUserModules
                    {
                        ModuleCode = lecture.ModuleCode,
                        UserId = lecture.UserId,
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        public static async Task<List<TblStaffLecture>> GetStaffLectures()
        {
            return await context.TblStaffLectures.ToListAsync();
        }

        public static async Task<List<TblStudent?>> GetStudentsByModule(string moduleCode)
        {
            var uids = await context.TblStudentLectures.Where(x => x.ModuleCode == moduleCode).Select(x => x.UserId).ToListAsync();
            List<TblStudent> output = new List<TblStudent>();

            foreach (var id in uids)
            {
                output.Add(GetStudentById(id).Result);
            }

            return output;
        }

        public static async Task<TblStudent?> GetStudentById(string userId)
        {
            return await context.TblStudents.Where(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        public static async Task<List<TblStudentLecture>> GetStudentLecturesByStaffId(string staffId)
        {
            var user = await context.TblStaffs.Where(x => x.StaffId == staffId).FirstOrDefaultAsync();

            if (user != null)
            {
                var lectures = await context.TblStaffLectures.Where(x => x.UserId == user.UserId).ToListAsync();
                List<TblStudentLecture> output = new List<TblStudentLecture>();

                foreach (var lecture in lectures)
                {
                    var item = await context.TblStudentLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefaultAsync();

                    if (item != null)
                    {
                        output.Add(item);
                    }
                }

                return output;
            }

            return null;
        }

        public static async Task<List<TblUser>> GetAllLecturers()
        {
            return await context.TblUsers.Where(x => x.TblStaff.RoleId == 1.ToString()).ToListAsync();
        }

        internal static void AddModule(string userId, TblModule module)
        {
            context.TblModules.Add(module);
            context.TblUserModules.Add(new TblUserModules
            {
                UserId = userId,
                ModuleCode = module.ModuleCode,
            });

            context.SaveChanges();
        }

        public static async Task<TblModule?> GetModule(string moduleCode)
        {
            return await context.TblModules.Where(x => x.ModuleCode == moduleCode).FirstOrDefaultAsync();
        }
    }
}
