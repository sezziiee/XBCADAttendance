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
                var user = ValidateSessionToken(token);

                if (user != null)
                {
                    var isConnected = InternetConnectivityService.IsInternetAvailableAsync().Result;

                    if (isConnected)
                    {
                        if (user.Role == "Lecturer")
                        {
                            return RedirectToAction("Index", "Staff");
                        } else if (user.Role == "Administrator")
                        {
                            return RedirectToAction("Index", "Admin");
                        } else
                        {
                            return RedirectToAction("Index", "Student", new { userID = user.UserID });
                        }
                    } else
                    {
                        if (user.Role == "Lecturer")
                        {
                            return RedirectToAction("LecturerQRCode", "Staff");
                        }else if (user.Role == "Student")
                        {
                            return RedirectToAction("StudentQRCode", "Student");
                        }
                    }
                }
            }

            return View();
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
