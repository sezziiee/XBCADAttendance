﻿using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult LectureReport(LectureReportViewModel model)
        {
            return View(model);
        }

        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public IActionResult CreateLecturer()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateLecturer(AddStaffViewModel model)
        {
            var viewModel = new AddStaffViewModel(model.LectureID, model.UserID, model.ModuleCode, model.Date, model.Start, model.Finish);
            
            
            return View(viewModel);
        }
    }
}
