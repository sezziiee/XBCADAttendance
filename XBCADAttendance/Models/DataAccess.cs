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

        //Read
        public List<TblStudent> GetAllStudents()
        {
            var data = context.TblStudents.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public List<TblLecture> GetAllLectures()
        {
            var data = context.TblLectures.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public List<TblModule> GetAllModules()
        {
            var data = context.TblModules.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public List<TblUser> GetAllUsers()
        {
            var data = context.TblUsers.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public List<TblStaff> GetAllStaff()
        {
            var data = context.TblStaffs.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        public List<TblRole> GetAllRoles()
        {
            var data = context.TblRoles.ToList();

            if (data != null)
            {
                return data;
            } else return null;
        }

        //Update
        public string UpdateStudent(string userID, string? studentNo, string? userName, string? passWord)
        {
            bool updateName = false;
            bool updatePassword = false;

            if (userName != null)
            {
                updateName = true;
            }

            if (passWord != null)
            {
                updatePassword = true;
            }

            if (updateName && updatePassword)
            {
                var student = context.TblStudents.Where(x => x.StudentNo == studentNo).FirstOrDefault();

                student.User.UserName = userName;
                student.User.Password = passWord;

            }else if(updateName && !updatePassword)
            {

            }else if(!updateName && updatePassword)
            {

            } else
            {
                return "No values were updated";
            }
        }

        //Delete
    }
}
