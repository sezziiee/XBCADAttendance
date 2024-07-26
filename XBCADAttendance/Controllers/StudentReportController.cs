using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class StudentReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
       
        public IActionResult StudentReport()
        {
            var report = new List<StudentReportViewModel>
            {
                new StudentReportViewModel
                {
                    ModuleCode = "XBCAD",
                    LectureDate = DateTime.Now,
                    ClassroomCode = "CR6",
                    ScanIn = DateTime.Now.AddHours(-10),
                    ScanOut = DateTime.Now.AddHours(-6)
                },
                new StudentReportViewModel
                {
                    ModuleCode = "PROG",
                    LectureDate = DateTime.Now.AddDays(-2),
                    ClassroomCode = "CR6",
                    ScanIn = DateTime.Now.AddHours(-9),
                    ScanOut = DateTime.Now.AddHours(-5)
                },

            };
            return View(report);
        }
       
    }
}
