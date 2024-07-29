using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
	public class LectureReportController : Controller
	{
		public DataAccess data = new DataAccess();

		public IActionResult Index()
		{
			var report = data.GetAllLectures();

            return View("LectureReport", report);
		}
	}
}
