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
        
        
        public AddStaffViewModel()
        {

        }

        public AddStaffViewModel(string lecturerID, string userID, string moduleCode, DateOnly date, TimeOnly start, TimeOnly end)
        {
            TblStaffLecture st = new TblStaffLecture
            {
                LectureId = lecturerID,
                UserId = userID,
                ModuleCode = moduleCode,
                Date = date,
                Start = start,
                Finish = end
            };

            DataAccess.AddLecture(st);
        }
        
    }

    
}
