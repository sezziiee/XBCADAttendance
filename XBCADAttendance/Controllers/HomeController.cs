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
            string token = HttpContext.Request.Cookies["session_token"];

            if (token != null)
            {
                var user = ValidateSessionToken(token);

                if (user != null)
                {
                    if (user.Role == "Lecturer")
                    {
                        return RedirectToAction("Index", "Staff");
                    } else if(user.Role == "Administrator") 
                    {
                        return RedirectToAction("Index", "Admin");
                    } else
                    {
                        return RedirectToAction("Index", "Student", new { userID = user.UserID });
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
                    ViewBag.Message = "Invalid login data.";
                    return View();
                }


                if (string.IsNullOrEmpty(model.identifier))
                {
                    ViewBag.Message = "Please enter your student/Staff number.";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.password))
                {
                    ViewBag.Message = "Please enter a password.";
                    return View(model);
                }

                if (model.identifier.Length < 10)
                {
                    if (!model.identifier.ToLower().StartsWith("st"))
                    {
                        model.identifier = "ST" + model.identifier;
                    } else
                    {
                        ViewBag.Message = "Please enter a valid student number.";
                        return View(model);
                    }
                }

                string? message = await DataAccess.LoginStudent(HttpContext, model);

                ViewBag.Message = message;

                if (message == "Success")
                {
                    // Generate a session token (e.g., JWT or custom token)
                    var sessionToken = GenerateSessionToken(model.identifier, message);

                    // Store the session token in an HTTP-only, secure cookie
                    Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                    {
                        HttpOnly = true, // Ensure JavaScript cannot access it
                        Secure = true,   // Only send cookie over HTTPS
                        SameSite = SameSiteMode.Strict, // Prevent CSRF attacks
                        Expires = DateTime.UtcNow.AddHours(1) // Set expiration date
                    });

                    return RedirectToAction("Index", "Student", new { userID = model.identifier });
                } else
                {
                    return View(model);
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ViewBag.Message = "An error occurred while processing your request. Please try again later.";
                return View(model);
            }
        }

        /*
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

                if (message == "Success")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Student", new { userID = model.identifier }) });
                }

                return Json(new { success = false, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }*/

        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.identifier))
            {
                ViewBag.Message = "Please enter your lecturer number.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.password))
            {
                ViewBag.Message = "Please enter a password.";
                return View(model);
            }

            string? message = await DataAccess.LoginStaff(HttpContext, model);

            ViewBag.Message = message;

            if (message == "Administrator")
            {
                // Generate a session token (e.g., JWT or custom token)
                var sessionToken = GenerateSessionToken(model.identifier, message);

                // Store the session token in an HTTP-only, secure cookie
                Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                {
                    HttpOnly = true, // Ensure JavaScript cannot access it
                    Secure = true,   // Only send cookie over HTTPS
                    SameSite = SameSiteMode.Strict, // Prevent CSRF attacks
                    Expires = DateTime.UtcNow.AddHours(1) // Set expiration date
                });

                return RedirectToAction("Index", "Admin");
            }
            if (message == "Lecturer")
            {
                // Generate a session token (e.g., JWT or custom token)
                var sessionToken = GenerateSessionToken(model.identifier, message);

                // Store the session token in an HTTP-only, secure cookie
                Response.Cookies.Append("session_token", sessionToken, new CookieOptions
                {
                    HttpOnly = true, // Ensure JavaScript cannot access it
                    Secure = true,   // Only send cookie over HTTPS
                    SameSite = SameSiteMode.Strict, // Prevent CSRF attacks
                    Expires = DateTime.UtcNow.AddHours(1) // Set expiration date
                });

                return RedirectToAction("Index", "Staff");
            } else
            {
                return View(model);
            }

        }

        public string GenerateSessionToken(string userID, string role)
        {
            // Simple token generation using GUID and the userID
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(userID + "|" + role + "|" + Guid.NewGuid().ToString()));
        }

        public UserAuth? ValidateSessionToken(string token)
        {
            try
            {
                // Decode the base64 string back to the user ID and GUID
                var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedString.Split('|');

                if (parts.Length == 3)
                {
                    var userID = parts[0];
                    var role = parts[1];

                    // Validate the userID
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

        /*
        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            try
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

                if (message == "Administrator")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Admin") });
                }
                else if (message == "Lecturer")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Staff") });
                }
                else
                {
                    return Json(new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "An error occurred while processing your request. Please try again later." });
            }
        }
        */



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
