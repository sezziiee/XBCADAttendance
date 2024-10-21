namespace XBCADAttendance.Models.ViewModels
{
    public class AddStaffViewModel
    {
        public int RoleID { get; set; }
        public string LecturerID { get; set; }
        
        
        public AddStaffViewModel()
        {

        }

        public AddStaffViewModel(int roleID, string lecturerID)
        {
			LecturerID = lecturerID;
            RoleID = roleID;
        }
        
    }

    
}
