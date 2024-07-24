namespace XBCADAttendance.Models
{
    public class StudentReportViewModel
    {
        public string ModuleCode { get; set; }
        public DateTime LectureDate { get; set; }
        public string ClassroomCode { get; set; }
        public DateTime ScanIn { get; set; }
        public DateTime ScanOut { get; set; }
    }
}
