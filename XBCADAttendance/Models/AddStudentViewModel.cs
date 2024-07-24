namespace XBCADAttendance.Models
{
    public class AddStudentViewModel
    {
        public string UserID { get; set; }
        public string StudentNo { get; set;}
        public string Username { get; set;}
        public string Password { get; set;}

        public AddStudentViewModel(string UserID, string StudentNo, string Username, string Password)
        {
            this.UserID = UserID;
            this.StudentNo = StudentNo;
            this.Username = Username;
            this.Password = Password;
        }
    }
}
