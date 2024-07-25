using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
	public class LectureReportController : Controller
	{
		List<LecturerReportViewModel> list = new List<LecturerReportViewModel>();

		public IActionResult Index()
		{
			var list = new LecturerReportViewModel("st10185639", "shap", "Y", 0);
			return View("LectureReport", list);
		}
	}
}
