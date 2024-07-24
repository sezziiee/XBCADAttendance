namespace XBCADAttendance.Models
{
    public class DataAccess
    {
        public DbWilContext context = new DbWilContext();
        public static DataAccess instance = null;
        
        public DataAccess()
        { 
            
        }

        public static DataAccess GetContext()
        {
            if (instance == null)
            {
                instance = new DataAccess();
            }

            return instance;
        }

        public string AddStudent(string UserID, string studentNo, string Username, string Password)
        {
            try
            {
                TblUser newUser = new TblUser
                {
                    UserId = UserID,
                    UserName = Username,
                    Password = Password
                    
                };

                TblStudent newStudent = new TblStudent
                {
                    StudentNo = studentNo,
                    UserId = UserID,
                };

                context.TblUsers.Add(newUser);
                context.TblStudents.Add(newStudent);
                context.SaveChanges();

                return "Success";
            
            }catch (Exception e)
            {
                return e.ToString();
            }
        }
    }
}
