using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class LectureReportController : Controller
	{
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Index()
        {
            return View();
        }

        //[Authorize(Policy = "AdminOnly")]
        public IActionResult LectureReport(LectureReportViewModel model)
		{
            return View(model);
		}

        //[Authorize(Policy = "AdminOnly")]
        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Profile()
        {
            return View();
        }

        //[Authorize(Policy = "AdminOnly")]
        public IActionResult AddLecture(LectureReportViewModel model)
        {
            return View(model);
        }
        
        [HttpGet]
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Create(TblStaffLecture lecture)
        {
            DataAccess.AddLecture(lecture);

            return View();

        }

		
	}
}
