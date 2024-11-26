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
        public List<TblStaffLecture> lstLectures = new List<TblStaffLecture>();
        public List<Student> lstStudents = new List<Student>();

        public string userId { get; set; }
        public string name { get; set; }
        public string staffId { get; set; }
        public string NextLecture { get; set; }

        public AttendanceByLecturerChart chart { get; set; }

        public LectureReportViewModel() { }

        public LectureReportViewModel(string userId)
        {
            this.userId = userId;
            user = DataAccess.GetUserById(userId).Result;
            staff = DataAccess.GetStaffByUID(userId).Result;
            staffId = staff.StaffId;
            name = user.UserName;

            lstModules = DataAccess.GetModulesById(userId).Result;

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

                string staffId = DataAccess.GetStaffIdFromUserId(userId).Result;

                chartData.Add(new DataPoint(lecturer.UserName, GetAttendanceByLecturer(staffId)));
            }

            chart = new AttendanceByLecturerChart(chartData, headings);

            lstLectures = DataAccess.GetStaffLecturesById(user.UserId).Result;

            NextLecture = GetNextLecture();
        }

        public int GetStudentAttendance(Student student)
        {
            int count = 0;

            foreach(var lecture in lstLectures)
            {
                foreach(var studLecture in student.TblStudentLectures)
                {
                    if (lecture.LectureId == studLecture.LectureId)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public int CalcLate(Student student)
        {
            int count = 0;

            foreach (var lecture in lstLectures)
            {
                foreach (var studLecture in student.TblStudentLectures)
                {
                    if (lecture.LectureId == studLecture.LectureId)
                    {
                        if(lecture.Start?.AddMinutes(5) < studLecture.ScanIn)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public int CalcAbsent(Student student)
        {
            return lstLectures.Count - GetStudentAttendance(student);
        }

        public float CalcAttendancePerc(Student student)
        {
            var late = CalcLate(student);
            var absent = CalcAbsent(student);
            var attended = GetStudentAttendance(student);
            var output = ((float)attended / (attended + absent + late)) * 100;
            return output;
        }

        private int GetAttendanceByLecturer(string staffId)
        {
            var lectures = DataAccess.GetStudentLecturesByStaffId(staffId).Result;
            return lectures.Where(x => x.LectureDate > DateOnly.FromDateTime(DateTime.Now.AddDays(-7))).ToList().Count();
        }

        public string GetAttendance(TblStudentLecture lecture)
        {
            return lecture.ScanOut != null ? "Attended" : "Absent";
        }

        public string GetNextLecture()
        {
            var lectures = DataAccess.GetStaffLectures().Result;

            var codes = lstModules.Select(x => x).ToList();
            lectures = lectures.Where(x => codes.Contains(x.ModuleCode)).ToList();

            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            var nextLecture = lectures
                .Where(x => x.Date > today)  
                .OrderBy(x => x.Date)  
                .FirstOrDefault(); 

            if (nextLecture != null)
            {
                var lectureDateTime = nextLecture.Date.ToDateTime(TimeOnly.MinValue); 
                var timeLeft = lectureDateTime - now;

                var days = timeLeft.Days;
                var hours = timeLeft.Hours;
                var minutes = timeLeft.Minutes;

                var timeLeftString = $"{(days > 0 ? $"{days} day(s) " : "")}{(hours > 0 ? $"{hours}h : " : "")}{minutes}m";

                return $"{timeLeftString}";
            }

            return "No upcoming lectures found.";
        }

        public byte[] GenerateQRCode()
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(userId, QRCodeGenerator.ECCLevel.Q);
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

        public List<Student> GetStudentsByLecturer(string staffId)
        {
            var students = new List<Student>();

            var lectures = DataAccess.GetStudentLecturesByStaffId(staffId).Result;
            foreach (var lecture in lectures)
            {
                var lectureStudents = DataAccess.GetStudentsFromLecture(lecture.LectureId).Result;
                foreach (var tblStudent in lectureStudents)
                {
                    Student student = new Student(tblStudent.StudentNo)
                    {
                        name = tblStudent.StudentNo,
                        attendancePerc = DataAccess.GetStudentAttendance(tblStudent.StudentNo).Result,
                        TblStudentLectures = DataAccess.GetAllLecturesByStudentNo(tblStudent.StudentNo).Result.ToList()
                    };
                    students.Add(student);
                }
            }

            return students.Distinct().ToList();
        }

        public int GetTotalStudents(TblStaffLecture staffLecture)
        {
            var studentLectures = DataAccess.GetStudentLecturesByLectureID(staffLecture.LectureId).Result;
            return studentLectures.Count;
        }

        //public async Task GetLecturesByLecturerAsync(string staffId)
        //{
        //    LecturerClasses = await DataAccess.GetStudentLecturesByStaffId(staffId);
        //}



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

