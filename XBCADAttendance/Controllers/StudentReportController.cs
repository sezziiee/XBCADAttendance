using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class StudentReportController : Controller
    {
       public DataAccess context = new DataAccess();
      
        public IActionResult StudentReport()
        {
            var report = context.GetIndividualStudents();
            return View(report);
        }
    }
}
