﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

namespace XBCADAttendance.Controllers
{
    public class StaffController : Controller
    {

        public IActionResult Index()
        {
            LectureReportViewModel model = new LectureReportViewModel(User.Identity.Name);
            return View(model);
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult LectureReport(LectureReportViewModel model)
        {
            return View(model);
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult StudentReport(LectureReportViewModel model)
        {
            return View(model);
        }

       
        [HttpGet]
        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Create()
        {
            CreateLectureViewModel model = new CreateLectureViewModel(User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Create(TblStaffLecture lecture)
        {
            lecture.LectureId = "L" + DataAccess.GetAllStaffLectures().Result.Count().ToString();
            lecture.UserId = User.Identity.Name;
            DataAccess.AddLecture(lecture);

            return RedirectToAction("Index", "Staff");
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult LecturerQRCode()
        {
            LectureReportViewModel newModel = new LectureReportViewModel();
            byte[] qrCodeImage = newModel.GenerateQRCode();

            return File(qrCodeImage, "image/png");
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Policy = "LecturerOnly")]
        public IActionResult Profile()
        {
            if (User.Identity.IsAuthenticated)
            {
                string userID = User.Identity.Name;

                if (!userID.IsNullOrEmpty())
                {
                    LectureReportViewModel newModel = new LectureReportViewModel(userID);
                    return View(newModel);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AddModule()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddModule(TblModule module)
        {
            try
            {
                DataAccess.Context.TblModules.Add(module);
                DataAccess.Context.SaveChanges();
                return RedirectToAction("Index", "Admin");
            } catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(module);
            }
        }
    }
}
