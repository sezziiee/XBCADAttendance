using Microsoft.Extensions.Configuration.UserSecrets;
using QRCoder;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Drawing;
using XBCADAttendance.Models;

namespace XBCADAttendance.Models
{
    public class LectureReportViewModel
    {
        public TblStaff staff { get; set; }
        public TblUser user { get; set; }
        public List<string>? lstModules = new List<string>();
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<Student> lstStudents = new List<Student>();

        public string userId { get; set; }
        public string name { get; set; }
        public string staffId { get; set; }

        public AttendanceByLecturerChart chart { get; set; }

        public LectureReportViewModel() { }

        public LectureReportViewModel(string userId)
        {
            this.userId = userId;
            user = DataAccess.GetUserById(userId).Result;
            staff = user.TblStaff;
            lstModules = DataAccess.GetModulesById(staff.UserId).Result;

            foreach (string moduleCode in lstModules)
            {
                var students = DataAccess.GetStudentsByModule(moduleCode).Result;

                foreach (var student in students)
                {
                    lstStudents.Add(new Student(student.StudentNo));
                }
            }

            var lecturers = DataAccess.GetAllLecturers().Result;

            List<DataPoint> chartData = new List<DataPoint>();
            List<string> headings = new List<string>();

            foreach (var lecturer in lecturers)
            {
                headings.Add(lecturer.UserName);

                string staffId = DataAccess.Context.TblStaffs.Where(x => x.UserId == lecturer.UserId).Select(x => x.StaffId).FirstOrDefault();

                chartData.Add(new DataPoint(lecturer.UserName, GetAttendanceByLecturer(staffId)));
            }

            chart = new AttendanceByLecturerChart(chartData, headings);

            lstLectures = DataAccess.GetStudentLecturesByStaffId(staff.StaffId).Result;
        }

        private int GetAttendanceByLecturer(string staffId)
        {
            var lectures = DataAccess.GetStudentLecturesByStaffId(staffId).Result.Where(x => x.LectureDate > DateOnly.FromDateTime(DateTime.Now.AddDays(-7))).ToList();
            return lectures.Count();
        }

        public string GetAttendance(TblStudentLecture lecture)
        {
            return lecture.ScanOut != null ? "Attended" : "Absent";
        }

        public byte[] GenerateQRCode()
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode("Lecturer", QRCodeGenerator.ECCLevel.Q);
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

        /*public void GetAllLectures()
        {
            var TblStudentLectures = DataAccess.GetContext().GetAllLectures();
            var tblStudents = DataAccess.GetContext().GetAllStudents();

            var output = TblStudentLectures.Join(tblStudents,
                     lecture => lecture.UserId,
                     student => student.UserId,
                     (lecture, student) => new LectureReport(
                        lecture.UserId,
                        student.StudentNo,
                        lecture.LectureDate.ToString(),
                        true,
                        lecture.ModuleCode)).ToList();
            lstReports.AddRange(output);
        }

        public string GetAttendance()
        {
            int attendance = 0;

            foreach (var report in lstReports)
            {
                attendance += report.attendance ? 1 : 0;
            }
            return attendance.ToString();
        }*/
    }

    public class Student
    {
        public string studentNo { get; set; }
        public string name { get; set; }
        public float attendancePerc {  get; set; }

        public List<TblStudentLecture> TblStudentLectures = new List<TblStudentLecture>();

        public Student(string studentNo)
        {
            this.studentNo = studentNo;
            name = DataAccess.GetUserById(DataAccess.GetIdByStudentNo(studentNo).Result).Result.UserName;
            TblStudentLectures = DataAccess.GetAllLecturesByStudentNo(studentNo).Result;
            attendancePerc = DataAccess.GetStudentAttendance(studentNo).Result;
        }

        public string DisplayAttendancePerc()
        {
            return $"{attendancePerc}%";
        }
    }

    public class AttendanceByLecturerChart 
    {
        public List<DataPoint> attendanceValues { get; set; }
        public List<string> headings { get; set; }

        public AttendanceByLecturerChart(List<DataPoint> attendanceValues, List<string> headings)
        {
            this.attendanceValues = attendanceValues;
            this.headings = headings;
        }
    }

}

