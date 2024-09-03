using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel.DataAnnotations;
using XBCADAttendance.Models;

namespace XBCADAttendance.Models
{
    public class LectureReportViewModel
    {
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<Student> lstStudents = new List<Student>();

        public LectureReportViewModel()
        {
            var students = DataAccess.GetContext().GetAllStudents();

            foreach (var student in students)
            {
                lstStudents.Add(new Student(student.StudentNo));
            }

            lstLectures = DataAccess.GetContext().GetAllLectures();
        }

        public string GetAttendance(TblStudentLecture lecture)
        {
            return lecture.ScanOut != null ? "Attended" : "Absent";
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
            name = DataAccess.GetContext().GetUserById(DataAccess.GetContext().GetIdByStudentNo(studentNo)).UserName;
            TblStudentLectures = DataAccess.GetContext().GetAllLecturesByStudentNo(studentNo);
            attendancePerc = DataAccess.GetContext().GetStudentAttendance(studentNo);
        }

        public string DisplayAttendancePerc()
        {
            return $"{attendancePerc}%";
        }
    }
}

