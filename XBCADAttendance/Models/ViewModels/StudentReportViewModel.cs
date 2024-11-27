
using System.Drawing.Imaging;
using System.Drawing;
using XBCADAttendance;
using QRCoder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace XBCADAttendance.Models
{
    public class StudentReportViewModel
    {
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<TblModule> lstModules = new List<TblModule>();
        public AttendancePieData AttendancePieData { get; set; }
        public AttendanceChartData AttendanceChartData { get; set; }

        public string[] statii = { "Absent", "Late", "Present" };

        public string UserID { get; set; }
        public string StudentNo { get; set; }
        public string Name { get; set; }

        public StudentReportViewModel()
        {

        }

        public StudentReportViewModel(string? userID = null)
        {
            if (userID != null)
            {
                UserID = userID;
                StudentNo = DataAccess.GetStudentNoById(UserID)!.Result;
                lstModules = DataAccess.GetUserModules(UserID).Result;
                lstLectures = DataAccess.GetAllLecturesByStudentNo(StudentNo).Result;
                Name = DataAccess.GetUserById(UserID)!.Result.UserName!;
            }

            List<DataPoint> attendanceVals = new List<DataPoint>();
            attendanceVals.Add(new DataPoint("Absent", GetMissedLectures()));
            attendanceVals.Add(new DataPoint("Late", GetLateLectures()));
            attendanceVals.Add(new DataPoint("Present", GetAttendedLectures()));
            AttendancePieData = new AttendancePieData(attendanceVals);

            List<DataPoint> dataPoints = new List<DataPoint>();
            List<string> headings = new List<string>();

            foreach (var module in lstModules)
            {
                headings.Add(module.ModuleCode);
                dataPoints.Add(new DataPoint(module.ModuleCode, GetAttendanceByModule(module.ModuleCode)));
            }

            AttendanceChartData = new AttendanceChartData(dataPoints, headings);
        }

        private int GetAttendanceByModule(string moduleCode)
        {
            var lectures = lstLectures.Where(x => x.ModuleCode == moduleCode).ToList();
            return lectures.Where(x => GetAttendance(x) != "Absent").Count();
        }

        public void ApplyFilters(string? moduleCode = null, DateOnly? start = null, DateOnly? end = null, string? status = null)
        {
            if (moduleCode != null)
            {
                lstLectures = lstLectures.Where(x => x.ModuleCode == moduleCode).ToList();
            }

            if (start != null)
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

        public void ApplyattendanceFilters(string? moduleCode = null, DateOnly? date = null)
        {
            if (moduleCode != null)
            {
                lstLectures = lstLectures.Where(x => x.ModuleCode == moduleCode).ToList();
            }

            if (date != null)
            {
                lstLectures = lstLectures.Where(x => x.LectureDate == date).ToList();
            }
        }

        public IEnumerable<SelectListItem> GetStatii()
        {
            return statii.Select(x => new SelectListItem { Value = x, Text = x }).ToList();
        }

        public string GetNextLecture()
        {
            var lectures = DataAccess.GetStaffLectures().Result;
            var codes = lstModules.Select(x => x.ModuleCode).ToList();

            lectures = lectures.Where(x => codes.Contains(x.ModuleCode)).ToList();

            var nextLecture = lectures.Where(x => x.Date >= DateOnly.FromDateTime(DateTime.Now)).OrderBy(x => x.Date).FirstOrDefault();

            return nextLecture != null ? $"{nextLecture.ModuleCode}: {nextLecture.Date}" : "No upcoming lectures found.";
        }

        public float CalcAttendencePerModule(string moduleCode)
        {
            var attendedLectures = GetAttendanceByModule(moduleCode);
            var totalLectures = DataAccess.GetStaffLectures().Result.Where(x => x.ModuleCode == moduleCode).ToList();

            return totalLectures.Count != 0 ? ((float)attendedLectures / totalLectures.Count) * 100 : 0;
        }

        /*public float CalcTotalAttendencePerModule(string moduleCode)
        {
            var attendedLectures = DataAccess.Context.TblStudentLectures.Where(x => x.ScanOut != null && x.UserId == UserID).ToList();
            var totalLectures = DataAccess.GetStaffLectures().Result.Where(x => x.ModuleCode == moduleCode).ToList();

            return totalLectures.Count != 0 ? ((float)attendedLectures.Count / totalLectures.Count) * 100 : 0;
        }*/

        public List<string> GetStudentModules()
        {
            return DataAccess.GetModulesByStudentNo(StudentNo).Result.Select(x => x.ModuleCode).ToList();
        }

        public IEnumerable<SelectListItem> GetModuleCodesForFilter()
        {
            return GetStudentModules().Select(x => new SelectListItem
            {
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
                if (lecture.ScanOut < lecture.ScanIn.AddMinutes(5))
                {
                    return "Absent";
                }
                return staffLecture.Start?.AddMinutes(5) > lecture.ScanIn ? "Present" : "Late";
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
                    if (actualLecture.ScanIn > lecture.Start?.AddMinutes(5) && actualLecture.ScanIn < lecture.Finish)
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        public int GetMissedLectures()
        {
            return GetTotalLectures() - (GetLateLectures() + GetAttendedLectures());
        }

        public int GetTotalLectures()
        {
            var staffLectures = DataAccess.GetStaffLectures().Result;
            List<TblStaffLecture> actualLectures = new List<TblStaffLecture>();

            foreach (var module in lstModules)
            {
                actualLectures.AddRange(staffLectures.Where(x => x.ModuleCode == module.ModuleCode).ToList());
            }

            return actualLectures.Count;
        }

        public float GetTotalAttendance()
        {
            int totalLectures = GetTotalLectures();

            if (totalLectures == 0)
            {
                return 0;
            }

            int attendedLectures = GetAttendedLectures();

            var attendancePercentage = ((float)attendedLectures / totalLectures) * 100;

            return attendancePercentage;
        }


        public int GetAttendedLectures()
        {
            int total = 0;

            var prelimAttended = lstLectures.Where(x => x.ScanOut != null).ToList();

            var allLectures = DataAccess.GetStaffLectures().Result;

            foreach (var lecture in prelimAttended)
            {
                var attendedLecture = allLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefault();
                var attendance = GetAttendance(lecture);

                if (attendance == "Present")
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

        public async Task UpdateUserCredentialsAsync(string userId, string newUserName, string newPassword)
        {
            if (userId != null)
            {
                await DataAccess.UpdateUser(userId, newUserName, newPassword);
            } else
            {
                throw new ArgumentException("User not found");
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

    public class AttendanceChartData
    {
        public List<DataPoint> attendanceValues { get; set; }
        public List<string> headings { get; set; }

        public AttendanceChartData(List<DataPoint> attendanceValues, List<string> headings)
        {
            this.attendanceValues = attendanceValues;
            this.headings = headings;
        }
    }

}
