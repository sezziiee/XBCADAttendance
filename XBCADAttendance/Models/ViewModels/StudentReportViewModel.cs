using XBCADAttendance;

namespace XBCADAttendance.Models
{
    public class StudentReportViewModel
    {
        public List<TblStudentLecture> lstLectures = new List<TblStudentLecture>();
        public List<TblModule> lstModules = new List<TblModule>();

        public string UserID { get; set; }
        public string StudentNo { get; set; }

        public StudentReportViewModel()
        {

        }

        public StudentReportViewModel(string? userID = null, string? studentNo = null)
        {
            if (userID != null)
            {
                UserID = userID;
                StudentNo = DataAccess.GetStudentNoById(UserID)!;
                lstModules = DataAccess.GetModulesByStudentNo(StudentNo);
                lstLectures = DataAccess.GetAllLecturesByStudentNo(StudentNo);
            }else if (studentNo != null)
            {
                StudentNo = studentNo;
                lstModules = DataAccess.GetModulesByStudentNo(StudentNo);
                lstLectures = DataAccess.GetAllLecturesByStudentNo(StudentNo);
                UserID = DataAccess.GetIdByStudentNo(StudentNo)!;
            }
        }

        public float CalcAttendencePerModule(string moduleCode)
        {
            return 15;
        }

        public string CalcDuration(TblStudentLecture lecture)
        {
            return (lecture.ScanOut - lecture.ScanIn).ToString();
        }

        public string GetAttendance(TblStudentLecture lecture)
        {
            return lecture.ScanOut != null ? "Attended" : "Absent";
        }

        public string GetModuleNameByCode(string moduleCode)
        {
            TblModule module = lstModules.Where(x => x.ModuleCode == moduleCode).FirstOrDefault();

            return module.ModuleName;
        }

        public int GetDaysAttended()
        {
            int totalDays = 0;
            var lectures = DataAccess.GetAllLecturesByStudentNo(StudentNo);
            var staffLectures = DataAccess.GetStaffLectures();

            var attendedLectures = lectures.Where(x => x.ScanOut != null ).ToList();
            var daysAttended = attendedLectures.DistinctBy(x => x.LectureDate);

            return daysAttended.Count();
        }

        public int GetLateLectures()
        {
            int total = 0;
            var lectures = DataAccess.GetAllLecturesByStudentNo(StudentNo);
            var attendedLectures = lectures.Where(x => x.ScanOut != null).ToList();

            var staffLectures = DataAccess.GetStaffLectures();

            foreach( var lecture in staffLectures)
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
            var staffLectures = DataAccess.GetStaffLectures();

            total += lstLectures.Where(x => x.ScanOut == null).Count();

            foreach(var lecture in staffLectures)
            {
                var actualLecture = lstLectures.Where(x => x.LectureId == lecture.LectureId).FirstOrDefault();

                if (actualLecture != null)
                {
                    if (actualLecture.ScanIn > lecture.Finish)
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        public float GetTotalAttendance()
        {
            var staffLectures = DataAccess.GetStaffLectures();
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
            var prelimAttended = lstLectures.Where(x => x.ScanOut != null).ToList();

            var allLectures = DataAccess.GetStaffLectures();

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



        /* public StudentReportViewModel(string userID, string studentNo, DateOnly lectureDate, string classroomCode, TimeOnly scanIn, TimeOnly scanOut, string moduleCode)
         {
             UserID = userID;
             StudentNo = studentNo;
             LectureDate = lectureDate;
             ClassroomCode = classroomCode;
             ScanIn = scanIn;
             ScanOut = scanOut;
             ModuleCode = moduleCode;
         }


         public DateOnly LectureDate { get; set; }
         public string ClassroomCode { get; set; }
         public TimeOnly ScanIn { get; set; }
         public TimeOnly ScanOut { get; set; }
         public string ModuleCode { get; set; }

         //Read
         public List<StudentReportViewModel> GetIndividualStudents(DataAccess context)
         {
             var data = context.GetAllLectures().Join(context.GetAllStudents(),
                lecture => lecture.UserId,
                student => student.UserId,
                (lecture, student) => new StudentReportViewModel(
                    lecture.UserId,
                    student.StudentNo,
                    lecture.LectureDate,
                    lecture.ClassroomCode,
                    lecture.ScanIn,
                    (TimeOnly)lecture.ScanOut!,
                    lecture.ModuleCode)).ToList();
             //Join TblStudentLecture and tblStudents and convert to a list.

             //Null check for data
             if (data != null)
             {
                 return data;
             }
             else return null;
         }
     }*/


    }
}
