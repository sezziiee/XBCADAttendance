namespace XBCADAttendance.Models
{
    public class StudentReportViewModel
    {
        public StudentReportViewModel(string userID, string studentNo, DateOnly lectureDate, string classroomCode, TimeOnly scanIn, TimeOnly scanOut, string moduleCode)
        {
            UserID = userID;
            StudentNo = studentNo;
            LectureDate = lectureDate;
            ClassroomCode = classroomCode;
            ScanIn = scanIn;
            ScanOut = scanOut;
            ModuleCode = moduleCode;
        }

        public string UserID { get; set; }
        public string StudentNo { get; set; }
        public DateOnly LectureDate { get; set; }
        public string ClassroomCode { get; set; }
        public TimeOnly ScanIn { get; set; }
        public TimeOnly ScanOut { get; set; }
        public string ModuleCode { get; set; }
    }
}
