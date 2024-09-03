using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    //[Authorize(Policy ="UserOnly")]
    public class StudentReportController : Controller
    {
        StudentReportViewModel? model;

        public IActionResult Index(string? userID = null)
        {
            if (model != null)
            {
                return View(model);
            } else
            {
                if (userID != null)
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);

                    model = newModel;
                    return View(newModel);
                } else
                {
                    //TODO Add error and redirect to homepage or login
                    return RedirectToAction();
                }
            }
        }
        public IActionResult StudentReport(string? userID = null, string? studentNo = null)
        {
            if (model != null)
            {
                return View(model);
            } else
            {
                if (userID != null)
                {
                    model = new StudentReportViewModel(userID);
                } else if (studentNo != null)
                {
                    model = new StudentReportViewModel(null, studentNo);

                } else
                {
                    //TODO Add error and redirect to homepage or login
                    return RedirectToAction();
                }

                return View(model);
            }
        }


        public IActionResult Profile()
        {
            return View();
        }


        public IActionResult Modules(string? userID = null)
        {
            if (model != null)
            {
                return View(model);
            } else
            {
                if (userID != null)
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);

                    model = newModel;
                    return View(newModel);
                } else
                {
                    //TODO Add error and redirect to homepage or login
                    return RedirectToAction();
                }
            }
        }
        public IActionResult AttendanceHistory(string? userID = null)
        {
            if (model != null)
            {
                return View(model);
            } else
            {
                if (userID != null)
                {
                    StudentReportViewModel newModel = new StudentReportViewModel(userID);

                    model = newModel;
                    return View(newModel);
                } else
                {
                    //TODO Add error and redirect to homepage or login
                    return RedirectToAction();
                }
            }
        }

    }
}
