﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class LectureReportController : Controller
	{
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LectureReport(LectureReportViewModel model)
		{
            return View(model);
		}
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(TblStaffLecture lecture)
        {
            DataAccess.AddLecture(lecture);

            return View();
        }
    }
}
