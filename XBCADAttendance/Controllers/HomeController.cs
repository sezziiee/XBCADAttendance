using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            DataAccess.GetInstance();
            string token = HttpContext.Request.Cookies["session_token"];

            if (token != null)
            {
                var user = ValidateToken(token);

                if (user != null)
                {
                    var isConnected = InternetConnectivityService.IsInternetAvailableAsync().Result;

                    if (isConnected)
                    {
                        var cachedUsers = LocalCacheService.GetAllCachedUsers();
                        foreach (var cachedUser in cachedUsers)
                        {
                            // Sync each cached UserAuth object with the server
                            var serverUser = DataAccess.GetUserById(cachedUser.UserAuth.UserID).Result;
                            string role = serverUser.TblStudent != null ? "Student" : "Lecturer";

                            var serverUserAuth = new UserAuth { UserID = serverUser.UserId };

                            if (serverUserAuth != null)
                            {
                                LocalCacheService.SaveUserToCache(serverUserAuth, cachedUser.SessionToken); // Update cache
                            }
                        }

                        RedirectBasedOnRole(user);
                    } else
                    {
                        RedirectToOfflineView(user);
                    }
                }
            }

            return View();
        }

        private IActionResult RedirectBasedOnRole(UserAuth user)
        {
            switch (user.Role)
            {
                case "Lecturer":
                    return RedirectToAction("Index", "Staff");
                case "Administrator":
                    return RedirectToAction("Index", "Admin");
                case "Student":
                    return RedirectToAction("Index", "Student", new { userID = user.UserID });
                default:
                    return View("Error"); // Handle unknown roles gracefully
            }
        }

        private IActionResult RedirectToOfflineView(UserAuth user)
        {
            switch (user.Role)
            {
                case "Lecturer":
                    return RedirectToAction("LecturerQRCode", "Staff");
                case "Student":
                    return RedirectToAction("StudentQRCode", "Student");
                default:
                    return View("OfflineError"); // Custom view for offline errors
            }
        }

        [HttpPost]
        public async Task<IActionResult> StudentLogin(LoginViewModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid login data." });
                }

                if (string.IsNullOrEmpty(model.identifier))
                {
                    return Json(new { success = false, message = "Please enter your student number." });
                }

                if (string.IsNullOrEmpty(model.password))
                {
                    return Json(new { success = false, message = "Please enter a password." });
                }

                if (model.identifier.Length < 10 && !model.identifier.ToLower().StartsWith("st"))
                {
                    model.identifier = "ST" + model.identifier;
                }
                
                

                string? message = await DataAccess.LoginStudent(HttpContext, model);

                ViewBag.Message = message;

                if (message == "Success")
                {
                    var sessionToken = GenerateSessionToken(model.identifier, message);

                    Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                    {
                        HttpOnly = true, 
                        Secure = true,  
                        SameSite = SameSiteMode.Strict, 
                        Expires = DateTime.UtcNow.AddHours(1) 
                    });

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Student", new { userID = model.identifier }) });
                } else
                {
                    return Json(new { success = false, message });
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }
        

        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.identifier))
            {
                return Json(new { success = false, message = "Please enter your staff number." });
            }

            if (string.IsNullOrEmpty(model.password))
            {
                return Json(new { success = false, message = "Please enter a password." });
            }

            string? message = await DataAccess.LoginStaff(HttpContext, model);

            ViewBag.Message = message;

            if (message == "Administrator")
            {
                var sessionToken = GenerateSessionToken(model.identifier, message);

                Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict, 
                    Expires = DateTime.UtcNow.AddHours(1) 
                });

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });
            }
            if (message == "Lecturer")
            {
                var sessionToken = GenerateSessionToken(model.identifier, message);

                Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true,   
                    SameSite = SameSiteMode.Strict, 
                    Expires = DateTime.UtcNow.AddHours(1) 
                });

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Staff") });
            } else
            {
                return Json(new { success = false, message });
            }

        }

        public string GenerateSessionToken(string userID, string role)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(userID + "|" + role + "|" + Guid.NewGuid().ToString()));
        }

        private UserAuth ValidateToken(string token)
        {
            bool isConnected = InternetConnectivityService.IsInternetAvailableAsync().Result;

            if (isConnected)
            {
                // Perform online validation and cache the UserAuth object
                return ValidateSessionTokenOnline(token);
            } else
            {
                // Perform offline validation using cached UserAuth
                return ValidateSessionTokenOffline(token);
            }
        }

        // Online validation
        private UserAuth ValidateSessionTokenOnline(string token)
        {
            // Replace this logic with the actual online token validation
            var userAuth = ValidateSessionToken(token); // Assume this retrieves the UserAuth object
            if (userAuth != null)
            {
                LocalCacheService.SaveUserToCache(userAuth, token); // Cache the UserAuth for offline use
            }

            return userAuth;
        }

        // Offline validation
        private UserAuth ValidateSessionTokenOffline(string token)
        {
            return LocalCacheService.GetUserByToken(token); // Retrieve from cache
        }


        public UserAuth? ValidateSessionToken(string token)
        {
            try
            {
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedString.Split('|');

                if (parts.Length == 3)
                {
                    var userID = parts[0];
                    var role = parts[1];

                    if (!string.IsNullOrEmpty(userID) && !string.IsNullOrEmpty(role))
                    {
                        return new UserAuth { UserID = userID, Role = role };
                    }
                }
            } catch
            {
                return null;
            }

            return null;
        }

        public class UserAuth
        {
            public string UserID { get; set; }
            public string Role { get; set; }
        }

        



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
