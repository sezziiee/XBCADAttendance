using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Models
{
    public class DataAccess
    {
        public DbWilContext context = new DbWilContext();
        public static DataAccess? instance = null;

        public DataAccess()
        {

        }

        public static DataAccess GetContext()
        {
            if (instance == null)
            {
                instance = new DataAccess();
            }

            return instance;
        }

        [ValidateAntiForgeryToken]
        public string LoginUser(HttpContext httpContext, LoginViewModel model)
        {
            if (model.identifier.Length > 5)//Check if id is for user/staff
            {
                var student = context.TblStudents.Where(x => x.StudentNo == model.identifier).FirstOrDefault();

                if (student != null)
                {
                    Hasher passwordHasher = new Hasher(student.User.Password!);

                    string userPassword = passwordHasher.GetHash();



                    if (passwordHasher.CompareHashedPasswords(userPassword, model.password))
                    {//Login logic

                        StoreUserCookies(httpContext, student.StudentNo, "Student");


                        return "Successful login";
                    }
                    else return "Incorrect password";

                }
                else
                {
                    return "Student not found";
                }

            }
            else
            {
                var staff = context.TblStaffs.Where(x => x.StaffId == model.identifier).FirstOrDefault();

                if (staff != null)
                {
                    if (staff.User.Password == model.password)
                    {//Add Login logic later
                        var role = GetAllRoles().Where(x => x.RoleId == staff.RoleId).Select(x => x.RoleName).ToString();
                        StoreUserCookies(httpContext, staff.StaffId, role);
                        return "Successful login";
                    }
                    else return "Incorrect password";
                }
                else
                {
                    return "Staff not found";
                }
            }
        }

        //Sign in and authentication
        // Stores user authentication cookies using ASP.NET Core's cookie authentication.
        public async Task StoreUserCookies(HttpContext httpContext, string studentNo, string identifier)
        {
            // Create a list of claims for the user with the student's number as the user's name.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, studentNo),
                new Claim(ClaimTypes.Role, identifier)
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

        public async Task Logout(HttpContext httpContext)
        {
            // Removing the users cookies
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        }
        //CRUD Operations

        //Create
        public string AddStudent(string userID, string studentNo, string userName, string passWord)
        {
            //userID = "Pleasewo";
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
        public List<TblStudent> GetAllStudents()
        {
            var data = context.TblStudents.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public List<TblLecture> GetAllLectures()
        {
            var data = context.TblLectures.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }


        public List<TblModule> GetAllModules()
        {
            var data = context.TblModules.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public List<TblUser> GetAllUsers()
        {
            var data = context.TblUsers.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public List<TblStaff> GetAllStaff()
        {
            var data = context.TblStaffs.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        public List<TblRole> GetAllRoles()
        {
            var data = context.TblRoles.ToList();

            if (data != null)
            {
                return data;
            }
            else return null;
        }

        //Update
        public string UpdateUser(string userID, string? userName, string? passWord)
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
        public string DeleteUser(string userID)
        {
            try
            {
                var user = context.TblUsers.Where(x => x.UserId == userID).FirstOrDefault();


                if (user != null)
                {
                    user.TblLectures.Clear();
                    user.TblStaffs.Clear();
                    user.TblStudents.Clear();
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

        public string DeleteLecture(string lectureID)
        {
            try
            {
                context.TblLectures.Where(x => x.LectureId == lectureID).ExecuteDelete();
                context.SaveChanges();
                return "Lecture deleted successfully";

            }
            catch (Exception e)
            {
                return $"Error: {e}";
            }
        }

        public string DeleteModule(string moduleCode)
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


    }
}
