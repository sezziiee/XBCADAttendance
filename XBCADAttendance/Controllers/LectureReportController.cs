using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
	public class LectureReportController : Controller
	{
		List<LecturerReportViewModel> lists = new List<LecturerReportViewModel>();

        private readonly DbWilContext context;

        //contructor passing the context into the controller
        public LectureReportController(DbWilContext _context)
        {
            context = _context;
        }

        public IActionResult Index()
		{
            var report = context.TblLectures.Join(context.TblStudents,
                         lecture => lecture.UserId,
                         student => student.UserId,
                         (lecture, student) => new LecturerReportViewModel(
                            lecture.UserId,
                            student.StudentNo,
                            lecture.LectureDate.ToString(),
                            "Yes",
                            8)).ToList();

            return View("LectureReport", report);
		}
	}
}
