﻿
using System.Drawing.Imaging;
using System.Drawing;
using XBCADAttendance;
using QRCoder;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XBCADAttendance.Models
{
    public class StudentReportViewModel
    {
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<TblModule> lstModules = new List<TblModule>();
        public AttendancePieData AttendancePieData { get; set; }

        public string[] statii = { "Absent", "Late", "Present" };

        public string UserID { get; set; }
        public string StudentNo { get; set; }
        public string Name { get; set; }

        public StudentReportViewModel()
        {

        }

        public StudentReportViewModel(string? userID = null, string? studentNo = null)
        {
            if (userID != null)
            {
                UserID = userID;
                StudentNo = DataAccess.GetStudentNoById(UserID)!.Result;
                lstModules = DataAccess.GetModulesByStudentNo(StudentNo).Result;
                lstLectures = DataAccess.GetAllLecturesByStudentNo(StudentNo).Result;
                Name = DataAccess.GetUserById(UserID)!.Result.UserName!;
            } else if (studentNo != null)
            {
                StudentNo = studentNo;
                lstModules = DataAccess.GetModulesByStudentNo(StudentNo).Result;
                lstLectures = DataAccess.GetAllLecturesByStudentNo(StudentNo).Result;
                UserID = DataAccess.GetIdByStudentNo(StudentNo).Result!;
                Name = DataAccess.GetUserById(UserID)!.Result.UserName!;
            }

            List<DataPoint> attendanceVals = new List<DataPoint>();
            attendanceVals.Add(new DataPoint("Absent", GetMissedLectures()));
            attendanceVals.Add(new DataPoint("Late", GetLateLectures()));
            attendanceVals.Add(new DataPoint("Present", GetAttendedLectures()));
            AttendancePieData = new AttendancePieData(attendanceVals);
        }

        public void ApplyFilters(string? moduleCode = null, DateOnly? start = null, DateOnly? end = null, string? status = null)
        {
            if (moduleCode != null)
            {
                lstLectures = lstLectures.Where(x => x.ModuleCode == moduleCode).ToList();
            }

            if(start != null)
            {
                lstLectures = lstLectures.Where(x => x.LectureDate > start).ToList();
            }

            if (end != null)
            {
                lstLectures = lstLectures.Where(x => x.LectureDate < end).ToList();
            }

            if (status != null)
            {
                lstLectures = lstLectures.Where(x => GetAttendance(x) == status).ToList();
            }
        }

        public IEnumerable<SelectListItem> GetStatii()
        {
            return statii.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
        }

        public float CalcAttendencePerModule(string moduleCode)
        {
            var attendedLectures = DataAccess.Context.TblStudentLectures.Where(x => x.ScanOut != null && x.UserId == UserID).ToList();
            var totalLectures = DataAccess.GetStaffLectures().Result.Where(x => x.ModuleCode == moduleCode).ToList();

            return ((float)attendedLectures.Count / totalLectures.Count) * 100;
        }

        public List<string> GetStudentModules()
        {
            return DataAccess.GetModulesByStudentNo(StudentNo).Result.Select(x => x.ModuleCode).ToList();
        }

        public IEnumerable<SelectListItem> GetModuleCodesForFilter()
        {
            return GetStudentModules().Select(x => new SelectListItem { 
                Value = x,
                Text = x
            }).ToList();
        }

        public string CalcDuration(TblStudentLecture lecture)
        {
            return (lecture.ScanOut - lecture.ScanIn).ToString();
        }

        public string GetAttendance(TblStudentLecture lecture)
        {
            if (lecture.ScanOut != null)
            {
                var staffLecture = DataAccess.Context.TblStaffLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefault();

                return staffLecture.Start < lecture.ScanIn.AddMinutes(5) ? "Present" : "Late";
            }

            return "Absent";
        }

        public string GetModuleNameByCode(string moduleCode)
        {
            TblModule module = lstModules.Where(x => x.ModuleCode == moduleCode).FirstOrDefault();

            return module.ModuleName;
        }

        public int GetDaysAttended()
        {
            int totalDays = 0;
            var lectures = DataAccess.GetAllLecturesByStudentNo(StudentNo).Result;
            var staffLectures = DataAccess.GetStaffLectures().Result;

            var attendedLectures = lectures.Where(x => x.ScanOut != null).ToList();
            var daysAttended = attendedLectures.DistinctBy(x => x.LectureDate);

            return daysAttended.Count();
        }

        public int GetLateLectures()
        {
            int total = 0;
            var lectures = DataAccess.GetAllLecturesByStudentNo(StudentNo).Result;
            var attendedLectures = lectures.Where(x => x.ScanOut != null).ToList();

            var staffLectures = DataAccess.GetStaffLectures().Result;

            foreach (var lecture in staffLectures)
            {
                var actualLecture = attendedLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefault();

                if (actualLecture != null)
                {
                    if (lecture.Start < actualLecture.ScanIn && actualLecture.ScanIn < lecture.Finish)
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        public int GetMissedLectures()
        {
            int total = 0;
            var staffLectures = DataAccess.GetStaffLectures().Result;
            var modules = lstLectures.Select(x => x.ModuleCode).Distinct().ToList();
            var attended = lstLectures.Where(x => x.ScanOut != null).ToList();

            total += lstLectures.Where(x => x.ScanOut == null).Count();

            foreach (var lecture in staffLectures)
            {
                if (modules.Contains(lecture.ModuleCode))
                {
                    total++;
                }
            }

            return total;
        }

        public float GetTotalAttendance()
        {
            var staffLectures = DataAccess.GetStaffLectures().Result;
            List<TblStaffLecture> actualLectures = new List<TblStaffLecture>();

            foreach (var module in lstModules)
            {
                actualLectures.AddRange(staffLectures.Where(x => x.ModuleCode == module.ModuleCode).ToList());
            }

            var total = ((float)GetAttendedLectures() / actualLectures.Count()) * 100;

            return total;
        }

        public int GetAttendedLectures()
        {
            int total = 0;

            var prelimAttended = lstLectures.Where(x => x.UserId == UserID && x.ScanOut != null).ToList();

            var allLectures = DataAccess.GetStaffLectures().Result;

            foreach (var lecture in prelimAttended)
            {
                var attendedLecture = allLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefault();

                if (attendedLecture.Finish > lecture.ScanIn)
                {
                    total++;
                }
            }

            return total;
        }

        public byte[] GenerateQRCode()
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(UserID, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            qrCodeImage.Save(ms, ImageFormat.Png);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }
    }

    public class AttendancePieData
    {
        public List<DataPoint> attendanceValues { get; set; }
        public AttendancePieData(List<DataPoint> attendanceValues)
        {
            this.attendanceValues = attendanceValues;
        }
    }
}
