using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
	public class LectureReportController : Controller
	{
		List<LecturerReportViewModel> lists = new List<LecturerReportViewModel>();

		public IActionResult Index()
		{
			var list = new LecturerReportViewModel("st10185639", "shap", "Y", 8);
			lists.Add(list);
			return View("LectureReport", lists);
		}
	}
}
