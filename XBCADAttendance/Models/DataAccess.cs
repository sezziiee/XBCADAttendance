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
using System.Collections.Concurrent;

namespace XBCADAttendance.Models
{
    public class DataAccess
    {
        private static readonly Queue<Func<Task>> requestQueue = new Queue<Func<Task>>();
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private static bool isProcessingQueue = false;
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

        // Enqueue a database operation
        public static Task<T> EnqueueOperation<T>(Func<Task<T>> operation)
        {
            var tcs = new TaskCompletionSource<T>();

            // Enqueue the operation for processing
            requestQueue.Enqueue(async () =>
            {
                try
                {
                    var result = await operation(); // Await the operation and capture its result
                    tcs.SetResult(result);         // Set the result on the TaskCompletionSource
                } catch (Exception ex)
                {
                    tcs.SetException(ex);          // Set the exception if the operation fails
                }
            });

            // Only call ProcessQueue if it's not already processing
            ProcessQueue();
            return tcs.Task;
        }

        private static void ProcessQueue()
        {
            lock (lockObj)
            {
                if (isProcessingQueue) return; // Prevent re-entrance while processing
                isProcessingQueue = true; // Flag that we're processing
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    Func<Task> operation;

                    lock (lockObj)
                    {
                        if (requestQueue.Count == 0)
                        {
                            isProcessingQueue = false; // No more operations, stop processing
                            break; // Exit the loop if no more tasks are in the queue
                        }

                        // Dequeue the next operation
                        operation = requestQueue.Dequeue();
                    }

                    await semaphore.WaitAsync();
                    try
                    {
                        await operation(); // Execute the operation
                    } finally
                    {
                        semaphore.Release();
                    }
                }
            });
        }

        // Wrapped method
        public static async Task<string> LoginStudent(HttpContext httpContext, LoginViewModel model)
        {
            return await EnqueueOperation(async () => await LoginStudentInternal(httpContext, model));
        }

        [ValidateAntiForgeryToken]
        private static async Task<string> LoginStudentInternal(HttpContext httpContext, LoginViewModel model)
        {
            if (model.identifier.Length == 10)
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
                            await StoreUserCookies(httpContext, student.UserId, "Student");
                            return "Success";
                        } else
                        {
                            return "Incorrect password";
                        }
                    }
                }
                return "Student not found";
            }
            return "Invalid student number";
        }

        // Wrapped method
        public static async Task<string> LoginStaff(HttpContext httpContext, LoginViewModel model)
        {
            return await EnqueueOperation(async () => await LoginStaffInternal(httpContext, model));
        }

        // Internal method
        [ValidateAntiForgeryToken]
        private static async Task<string> LoginStaffInternal(HttpContext httpContext, LoginViewModel model)
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
                            var allRoles = await GetAllRolesInternal();

                            var role = await Task.Run(() =>
                                    allRoles?
                                    .Where(x => x.RoleId == staff.RoleId)
                                    .Select(x => x.RoleName)
                                    .FirstOrDefault());

                            await StoreUserCookies(httpContext, staff.UserId, role ?? "Unknown");
                            return role ?? "Unknown";
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
        private static async Task StoreUserCookies(HttpContext httpContext, string id, string role)
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
            List<TblStudentLecture> studentLectures = await GetAllLecturesByStudentNoInternal(studentNo);
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
        // Wrapped method
        public static async Task<string> AddStudent(string userID, string studentNo, string userName, string passWord)
        {
            return await EnqueueOperation(async () => await AddStudentInternal(userID, studentNo, userName, passWord));
        }

        // Internal method
        private static async Task<string> AddStudentInternal(string userID, string studentNo, string userName, string passWord)
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
        // Wrapped method
        public static async Task<TblUser?> GetUserById(string userID)
        {
            return await EnqueueOperation(async () => await GetUserByIdInternal(userID));
        }

        // Internal method
        private static async Task<TblUser?> GetUserByIdInternal(string userID)
        {
            return await context.TblUsers.Where(x => x.UserId == userID).FirstOrDefaultAsync();
        }

        // Wrapped method
        public static async Task<List<string>?> GetModulesById(string userID)
        {
            return await EnqueueOperation(async () => await GetModulesByIdInternal(userID));
        }

        // Internal method
        private static async Task<List<string>?> GetModulesByIdInternal(string userID)
        {
            var user = await GetUserByIdInternal(userID);

            if (user != null)
            {
                return await context.TblUserModules.Where(x => x.UserId == userID).Select(x => x.ModuleCode).Distinct().ToListAsync();
            }

            return null;
        }

        // Wrapped method
        public static async Task<TblUser?> GetUserByStudentNo(string studentNo)
        {
            return await EnqueueOperation(async () => await GetUserByStudentNoInternal(studentNo));
        }

        // Internal method
        private static async Task<TblUser?> GetUserByStudentNoInternal(string studentNo)
        {
            return await context.TblUsers.Where(x => x.TblStudent.StudentNo == studentNo).FirstOrDefaultAsync();
        }

        // Wrapped method
        public static async Task<List<TblStudent>> GetAllStudents()
        {
            return await EnqueueOperation(async () => await GetAllStudentsInternal());
        }

        // Internal method
        private static async Task<List<TblStudent>> GetAllStudentsInternal()
        {
            var data = await context.TblStudents.ToListAsync();

            return data ?? new List<TblStudent>();  // Ensure the return value is never null
        }

        // Wrapped method
        public static async Task<List<TblStudentLecture>?> GetStudentLectures(string userID)
        {
            return await EnqueueOperation(async () => await GetStudentLecturesInternal(userID));
        }

        // Internal method
        private static async Task<List<TblStudentLecture>?> GetStudentLecturesInternal(string userID)
        {
            var lectures = await context.TblStudentLectures.Where(x => x.UserId == userID).ToListAsync();

            return lectures ?? new List<TblStudentLecture>();  // Ensure non-null return value
        }

        // Wrapped method
        public static async Task<float> GetStudentAttendance(string studentNo)
        {
            return await EnqueueOperation(async () => await GetStudentAttendanceInternal(studentNo));
        }

        // Internal method
        private static async Task<float> GetStudentAttendanceInternal(string studentNo)
        {
            var student = await context.TblStudents.Where(x => x.StudentNo == studentNo).FirstOrDefaultAsync();

            if (student != null)
            {
                var totalLectures = await GetStudentLecturesInternal(student.UserId);
                var attendance = await context.TblStudentLectures.Where(x => x.UserId == student.UserId && x.ScanOut != null).ToListAsync();

                if (totalLectures.Count > 0)
                {
                    return ((float)attendance.Count / totalLectures.Count) * 100;
                }
            }

            return 0;
        }

        // Wrapped method
        public static async Task<string?> GetIdByStudentNo(string studentNo)
        {
            return await EnqueueOperation(async () => await GetIdByStudentNoInternal(studentNo));
        }

        // Internal method
        private static async Task<string?> GetIdByStudentNoInternal(string studentNo)
        {
            var students = await context.TblUsers.Where(x => x.TblStudent != null).Select(x => x.TblStudent).ToListAsync();

            return students.Where(x => x!.StudentNo == studentNo).Select(x => x!.UserId).FirstOrDefault();
        }

        // Wrapped method
        public static async Task<string?> GetStudentNoById(string userID)
        {
            return await EnqueueOperation(async () => await GetStudentNoByIdInternal(userID));
        }

        // Internal method
        private static async Task<string?> GetStudentNoByIdInternal(string userID)
        {
            return await context.TblUsers
                .Where(x => x.UserId == userID && x.TblStudent != null)
                .Select(x => x.TblStudent!.StudentNo)
                .FirstOrDefaultAsync();
        }

        // Wrapped method
        public static async Task<List<TblStudentLecture>> GetAllLectures()
        {
            return await EnqueueOperation(async () => await GetAllLecturesInternal());
        }

        // Internal method
        private static async Task<List<TblStudentLecture>> GetAllLecturesInternal()
        {
            var data = await context.TblStudentLectures.ToListAsync();
            return data; // No need for null check since ToListAsync() returns an empty list if no records are found
        }

        // Wrapped method
        public static async Task<List<TblStaffLecture>> GetAllStaffLectures()
        {
            return await EnqueueOperation(async () => await GetAllStaffLecturesInternal());
        }

        // Internal method
        private static async Task<List<TblStaffLecture>> GetAllStaffLecturesInternal()
        {
            var data = await context.TblStaffLectures.ToListAsync();
            return data; // No need for null check since ToListAsync() returns an empty list if no records are found
        }

        // Wrapped method
        public static async Task<List<TblStudent>> GetStudentsFromLecture(string lectureId)
        {
            return await EnqueueOperation(async () => await GetStudentsFromLectureInternal(lectureId));
        }

        // Internal method
        private static async Task<List<TblStudent>> GetStudentsFromLectureInternal(string lectureId)
        {
            List<TblStudent> output = new List<TblStudent>();

            var lectures = await context.TblStudentLectures.Where(x => x.LectureId == lectureId).ToListAsync();
            List<string> studentIds = lectures.Select(x => x.UserId).ToList();

            foreach (var id in studentIds)
            {
                var student = await context.TblStudents.Where(x => x.UserId == id).FirstOrDefaultAsync();
                if (student != null)
                {
                    output.Add(student);
                }
            }

            return output;
        }

        // Wrapped method
        public static async Task<List<TblStudentLecture>?> GetAllLecturesByStudentNo(string studentNo)
        {
            return await EnqueueOperation(async () => await GetAllLecturesByStudentNoInternal(studentNo));
        }

        // Internal method
        private static async Task<List<TblStudentLecture>?> GetAllLecturesByStudentNoInternal(string studentNo)
        {
            var studentID = await context.TblUsers
                .Where(x => x.TblStudent != null && x.TblStudent.StudentNo == studentNo)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(studentID))
            {
                return null; // No student found
            }

            var data = await context.TblStudentLectures
                .Where(x => x.UserId == studentID)
                .ToListAsync();

            return data;
        }

        // Wrapped method
        public static async Task<List<TblModule>?> GetAllModules()
        {
            return await EnqueueOperation(async () => await GetAllModulesInternal());
        }

        // Internal method
        private static async Task<List<TblModule>?> GetAllModulesInternal()
        {
            var data = await context.TblModules.ToListAsync();
            return data;
        }

        // Wrapped method
        public static async Task<List<TblModule>> GetModulesByStudentNo(string studentNo)
        {
            return await EnqueueOperation(async () => await GetModulesByStudentNoInternal(studentNo));
        }

        // Internal method
        private static async Task<List<TblModule>> GetModulesByStudentNoInternal(string studentNo)
        {
            string userId = await GetIdByStudentNoInternal(studentNo)!;
            var lectures = await context.TblStudentLectures
                .Where(x => x.UserId == userId)
                .Select(x => x.ModuleCode)
                .Distinct()
                .ToListAsync();

            var modules = await context.TblModules.ToListAsync();

            List<TblModule> lstModules = new List<TblModule>();

            foreach (var lecture in lectures)
            {
                var studentModules = modules.Where(x => x.ModuleCode == lecture).ToList();
                lstModules.AddRange(studentModules);
            }

            return lstModules;
        }

        // Wrapped method
        public static async Task<List<TblUser>> GetAllUsers()
        {
            return await EnqueueOperation(async () => await GetAllUsersInternal());
        }

        // Internal method
        private static async Task<List<TblUser>> GetAllUsersInternal()
        {
            var data = await context.TblUsers.ToListAsync();

            if (data != null)
            {
                return data;
            }
            return null;
        }

        // Wrapped method
        public static async Task<List<TblStaff>> GetAllStaff()
        {
            return await EnqueueOperation(async () => await GetAllStaffInternal());
        }

        // Internal method
        private static async Task<List<TblStaff>> GetAllStaffInternal()
        {
            var data = await context.TblStaffs.ToListAsync();

            if (data != null)
            {
                return data;
            }
            return null;
        }

        // Wrapped method
        public static async Task<List<TblRole>?> GetAllRoles()
        {
            return await EnqueueOperation(async () => await GetAllRolesInternal());
        }

        // Internal method
        private static async Task<List<TblRole>?> GetAllRolesInternal()
        {
            var data = await context.TblRoles.ToListAsync();

            if (data != null)
            {
                return data;
            }
            return null;
        }

        //Update
        // Wrapped method
        public static async Task<string> UpdateUser(string userID, string? userName, string? passWord)
        {
            return await EnqueueOperation(async () => await UpdateUserInternal(userID, userName, passWord));
        }

        // Internal method
        private static async Task<string> UpdateUserInternal(string userID, string? userName, string? passWord)
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
        // Wrapped method
        public static async Task<string> DeleteUser(string userID)
        {
            return await EnqueueOperation(async () => await DeleteUserInternal(userID));
        }

        // Internal method
        private static async Task<string> DeleteUserInternal(string userID)
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

        // Wrapped method
        public static async Task<string> DeleteLecture(string lectureID)
        {
            return await EnqueueOperation(async () => await DeleteLectureInternal(lectureID));
        }

        // Internal method
        private static async Task<string> DeleteLectureInternal(string lectureID)
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

        // Wrapped method
        public static async Task<string> DeleteModule(string moduleCode)
        {
            return await EnqueueOperation(async () => await DeleteModuleInternal(moduleCode));
        }

        // Internal method
        private static async Task<string> DeleteModuleInternal(string moduleCode)
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

        // Wrapped method
        public static async Task AddLecture(TblStaffLecture lecture)
        {
            await EnqueueOperation(async () => await AddLectureInternal(lecture));
        }

        // Internal method
        private static async Task<string> AddLectureInternal(TblStaffLecture lecture)
        {
            try
            {
                // Add error handling
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

                return "Success";

            } catch (Exception e) 
            { 
                return e.Message;
            }
        }

        // Wrapped method
        public static async Task<List<TblStaffLecture>> GetStaffLectures()
        {
            return await EnqueueOperation(async () => await GetStaffLecturesInternal());
        }

        // Internal method
        private static async Task<List<TblStaffLecture>> GetStaffLecturesInternal()
        {
            return await context.TblStaffLectures.ToListAsync();
        }

        // Wrapped method
        public static async Task<List<TblStudent?>> GetStudentsByModule(string moduleCode)
        {
            return await EnqueueOperation(async () => await GetStudentsByModuleInternal(moduleCode));
        }

        // Internal method
        private static async Task<List<TblStudent?>> GetStudentsByModuleInternal(string moduleCode)
        {
            var uids = await context.TblStudentLectures.Where(x => x.ModuleCode == moduleCode).Select(x => x.UserId).ToListAsync();
            List<TblStudent> output = new List<TblStudent>();

            foreach (var id in uids)
            {
                output.Add(await GetStudentByIdInternal(id)); // Use 'await' here instead of '.Result' to avoid blocking
            }

            return output;
        }

        // Wrapped method
        public static async Task<TblStudent?> GetStudentById(string userId)
        {
            return await EnqueueOperation(async () => await GetStudentByIdInternal(userId));
        }

        // Internal method
        private static async Task<TblStudent?> GetStudentByIdInternal(string userId)
        {
            return await context.TblStudents.Where(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        // Wrapped method
        public static async Task<List<TblStudentLecture>> GetStudentLecturesByStaffId(string staffId)
        {
            return await EnqueueOperation(async () => await GetStudentLecturesByStaffIdInternal(staffId));
        }

        // Internal method
        private static async Task<List<TblStudentLecture>> GetStudentLecturesByStaffIdInternal(string staffId)
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

        // Wrapped method
        public static async Task<List<TblUser>> GetAllLecturers()
        {
            return await EnqueueOperation(async () => await GetAllLecturersInternal());
        }

        // Internal method
        private static async Task<List<TblUser>> GetAllLecturersInternal()
        {
            return await context.TblUsers.Where(x => x.TblStaff.RoleId == 1.ToString()).ToListAsync();
        }

        // Wrapped method
        public static async Task AddModule(string userId, TblModule module)
        {
            await EnqueueOperation(async () => await AddModuleInternal(userId, module));
        }

        // Internal method
        private static async Task<string> AddModuleInternal(string userId, TblModule module)
        {
            try
            {
                context.TblModules.Add(module);
                context.TblUserModules.Add(new TblUserModules
                {
                    UserId = userId,
                    ModuleCode = module.ModuleCode,
                });

                await context.SaveChangesAsync();

                return "Success";

            } catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Wrapped method
        public static async Task<TblModule?> GetModule(string moduleCode)
        {
            return await EnqueueOperation(async () => await GetModuleInternal(moduleCode));
        }

        // Internal method
        private static async Task<TblModule?> GetModuleInternal(string moduleCode)
        {
            return await context.TblModules.Where(x => x.ModuleCode == moduleCode).FirstOrDefaultAsync();
        }
    }
}
