﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class StudentReportController : Controller
    {      
        public IActionResult StudentReport(StudentReportViewModel model)
        {
            var report = model.GetIndividualStudents(DataAccess.GetContext());
            return View(report);
        }
    }
}
