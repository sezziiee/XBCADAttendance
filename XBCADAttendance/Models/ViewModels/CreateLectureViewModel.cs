
namespace XBCADAttendance.Models.ViewModels
{
    public class CreateLectureViewModel
    {
        public List<string> lstModules;

        public DateOnly date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public string moduleCode { get; set; }

        public CreateLectureViewModel(string userId)
        {
            var modules = DataAccess.GetAllModules().Result;
            lstModules = modules.Select(x => x.ModuleCode.ToString()).ToList();
        }
    }
}
