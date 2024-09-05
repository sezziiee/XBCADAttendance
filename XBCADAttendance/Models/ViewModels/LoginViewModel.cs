namespace XBCADAttendance.Models
{
    public class LoginViewModel
    {
        public string identifier { get; set; }
        public string password { get; set; }

        public LoginViewModel()
        {

        }
        public LoginViewModel(string identifier, string password)
        {
            this.identifier = identifier;
            this.password = password;
        }
    }
}
