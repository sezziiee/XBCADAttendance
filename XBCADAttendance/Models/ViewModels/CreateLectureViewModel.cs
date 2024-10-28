namespace XBCADAttendance.Models.ViewModels
{
    public class CreateLectureViewModel
    {
        public List<string> lstModules; 

        public DateOnly date { get; set; }

        public string moduleCode { get; set; }

        public CreateLectureViewModel(string userId) 
        {
            lstModules = DataAccess.GetModulesById(userId);
        }
    }
}
