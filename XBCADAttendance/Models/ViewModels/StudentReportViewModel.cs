
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
                StudentNo = DataAccess.GetContext().GetStudentNoById(UserID)!;
                lstModules = DataAccess.GetContext().GetModulesByStudentNo(StudentNo);
                lstLectures = DataAccess.GetContext().GetAllLecturesByStudentNo(StudentNo);
            }else if (studentNo != null)
            {
                StudentNo = studentNo;
                lstModules = DataAccess.GetContext().GetModulesByStudentNo(StudentNo);
                lstLectures = DataAccess.GetContext().GetAllLecturesByStudentNo(StudentNo);
                UserID = DataAccess.GetContext().GetIdByStudentNo(StudentNo)!;
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
