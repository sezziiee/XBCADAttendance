using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class LectureReportController : Controller
	{

		public IActionResult LectureReport(LectureReportViewModel model)
		{
            return View(model);
		}
	}
}
