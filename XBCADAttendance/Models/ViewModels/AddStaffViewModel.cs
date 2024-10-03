namespace XBCADAttendance.Models.ViewModels
{
    public class AddStaffViewModel
    {
        public string LectureID { get; set; }
        public string UserID { get; set; }
        public string ModuleCode { get; set; }
        public DateOnly Date {get; set;}
        public TimeOnly Start { get; set;}
        public TimeOnly Finish { get; set;}
    }
}
