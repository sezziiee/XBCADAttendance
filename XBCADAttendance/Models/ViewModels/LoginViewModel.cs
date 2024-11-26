using XBCADAttendance.Models;

public class LoginViewModel
{
    public string identifier { get; set; }
    public string password { get; set; }
    public string userId { get; private set; }

    public LoginViewModel() {}

    public LoginViewModel(string identifier, string password)
    {
        this.identifier = identifier;
        this.password = password;
    }

    public async Task InitializeAsync()
    {
        if (identifier.Length > 5)
        {
            var user = await DataAccess.GetUserByStudentNo(identifier);
            if (user == null)
                throw new InvalidOperationException("User not found for the provided identifier.");

            userId = user.UserId;
        } else
        {
            var user = await DataAccess.GetUserByStaffNo(identifier);
            if (user == null)
                throw new InvalidOperationException("User not found for the provided identifier.");

            userId = user.UserId;
        }
        
    }
}
