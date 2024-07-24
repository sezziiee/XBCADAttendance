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

        public string AddStudent(string userID, string studentNo, string userName, string passWord)
        {
            //userID = "TestUser";
            if (userID != null && studentNo != null && userName != null && passWord != null)
            {
                try
                {
                    TblUser newUser = new TblUser
                    {
                        UserId = userID,
                        UserName = userName,
                        Password = passWord

                    };

                    TblStudent newStudent = new TblStudent
                    {
                        StudentNo = studentNo,
                        UserId = userID,
                    };

                    context.TblUsers.Add(newUser);
                    context.TblStudents.Add(newStudent);
                    context.SaveChanges();

                    return "Success";

                } catch (Exception e)
                {
                    return e.ToString();
                }
            } else return "Please Fill in all fields";
        }
    }
}
