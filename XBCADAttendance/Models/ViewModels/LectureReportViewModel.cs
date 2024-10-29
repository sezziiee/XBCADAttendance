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

        public string UserID { get; set; }
        public string Name { get; set; }

        public string staffId { get; set; }

        public List<string>? lstModules = new List<string>();
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<Student> lstStudents = new List<Student>();

        public LectureReportViewModel() { }

        public LectureReportViewModel(string userId)
        {
            UserID = userId;
            user = DataAccess.GetUserById(userId).Result;
            staff = user.TblStaff;
            staffId = staff.StaffId;
            Name = user.UserName;

            lstModules = DataAccess.GetModulesById(userId).Result;

            foreach (string moduleCode in lstModules)
            {
                var students = DataAccess.GetStudentsByModule(moduleCode).Result;

                foreach (var student in students)
                {
                    lstStudents.Add(new Student(student.StudentNo));
                }
            }
            
            lstLectures = DataAccess.GetStudentLecturesByStaffId(staff.StaffId).Result;
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
}

