using Google.Apis.Admin.Directory.directory_v1.Data;
using System.Drawing;

namespace XBCADAttendance.Models.ViewModels
{
    public class AdminViewModel
    {
        public List<TblUser> Users { get; set; }
        public List<TblStudent> Students { get; set; }
        public List<TblStaff> Staff { get; set; }
        public List<TblStaffLecture> StaffLectures { get; set; }

        public List<TblRole> lstRoles = new List<TblRole>();

        public List<DataPoint> LecturerAttendanceChartData { get; set; }
        public List<DataPoint> ModuleAttendanceChartData { get; set; }

        public AdminViewModel() 
        {
            Users = DataAccess.GetAllUsers().Result;
            Students = DataAccess.GetAllStudents().Result;
            Staff = DataAccess.GetAllStaff().Result;
            StaffLectures = DataAccess.GetStaffLectures().Result;

            LecturerAttendanceChartData = new List<DataPoint>();
            ModuleAttendanceChartData = new List<DataPoint>();

            var lecturers = DataAccess.GetAllLecturers().Result;

            foreach (var lecturer in lecturers)
            {
                string staffId = DataAccess.Context.TblStaffs
                    .Where(x => x.UserId == lecturer.UserId)
                    .Select(x => x.StaffId)
                    .FirstOrDefault();

                var attendance = GetAverageAttendanceByLecturer(staffId);
               
                LecturerAttendanceChartData.Add(new DataPoint(lecturer.UserName, (int)Math.Round(attendance)));
            }

            // Populate average attendance data for modules
            var modules = DataAccess.GetAllModules().Result;
            foreach (var module in modules)
            {
                var attendance = GetAverageAttendanceByModule(module.ModuleCode);
                ModuleAttendanceChartData.Add(new DataPoint(module.ModuleCode, (int)Math.Round(attendance)));
            }
        }
        private double GetAverageAttendanceByLecturer(string staffId)
        {
            var lectures = DataAccess.GetStudentLecturesByStaffId(staffId).Result;
            int totalStudents = lectures.Sum(lecture => DataAccess.GetStudentsFromLecture(lecture.LectureId).Result.Count);
            return totalStudents / (double)lectures.Count;
        }

        private double GetAverageAttendanceByModule(string moduleCode)
        {
            var lectures = DataAccess.GetLecturesByModuleCode(moduleCode).Result;
            if (lectures.Count == 0)
            {
                return 0;
            }
            int totalStudents = lectures.Sum(lecture => DataAccess.GetStudentsFromLecture(lecture.LectureId).Result.Count);
            return totalStudents / (double)lectures.Count;
        }
       
        public string GetID(TblUser user)
        {
            var student = Students.Where(x => x.UserId == user.UserId).FirstOrDefault();
            if (student != null)
            {
                return student.UserId;
            } else
            {
                var staff = Staff.Where(x => x.UserId == user.UserId).FirstOrDefault();
                if (staff != null)
                {
                    return staff.StaffId;
                }
            }

            return "ERROR";
        }

        public string GetRole(TblUser user)
        {
            var student = Students.Where(x => x.UserId == user.UserId).FirstOrDefault();
            if (student != null)
            {
                return "Student";
            } else
            {
                var staff = Staff.Where(x => x.UserId == user.UserId).FirstOrDefault();
                if (staff != null)
                {
                    var roles = DataAccess.GetAllRoles().Result;

                    return roles.Where(x => x.RoleId == staff.RoleId).Select(x => x.RoleName).FirstOrDefault();
                }
            }

            return "ERROR";
        }

        public string GetLecturer(TblStaffLecture lecture)
        {
            var lecturer = DataAccess.Context.TblStaffs.Where(x => x.UserId == lecture.UserId).Select(x => x.User).FirstOrDefault();

            if (lecturer != null)
            {
                return lecturer.UserName;
            }

            return "Error";
        }

        public async Task<List<TblStudent>> GetStudentsFromLecture(string lectureId)
        {
            return await DataAccess.GetStudentsFromLecture(lectureId);
        }

        public async Task UpdateUserCredentialsAsync(string userId, string newUserName, string newPassword)
        {
            if (userId != null)
            {
                await DataAccess.UpdateUser(userId, newUserName, newPassword);
            }
            else
            {
                throw new ArgumentException("User not found");
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

        public class StudentAttendance
        {
            public string StudentName { get; set; }
            public double AttendancePercentage { get; set; }
        }

        public async Task<List<StudentAttendance>> GetStudentsWithAttendance()
        {
            var studentAttendanceList = new List<StudentAttendance>();
            var totalLectures = await DataAccess.GetAllLectures();

            foreach (var student in Students)
            {
                var attendedLectures = await DataAccess.GetAllLecturesByStudentNo(student.StudentNo) ?? new List<TblStudentLecture>();

                var attendancePercentage = ((float)attendedLectures.Count / totalLectures.Count) * 100;
       
                studentAttendanceList.Add(new StudentAttendance
                {
                    StudentName = student.User.UserName,
                    AttendancePercentage = Math.Round(attendancePercentage, 2)
                });
            }

            return studentAttendanceList;
        }
    }
}
