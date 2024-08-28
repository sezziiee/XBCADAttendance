using Microsoft.Extensions.Configuration.UserSecrets;
using XBCADAttendance.Models;

namespace XBCADAttendance.Models
{
    public class LectureReportViewModel
    {
        public List<LectureReport> lstReports = new List<LectureReport>();

        public LectureReportViewModel() 
        {
            GetAllLectures();
        }

        public int CalculateDaysAttended()
        {
            var day = from x in DataAccess.GetContext().GetAllLectures()
                      where x.ScanOut != null
                      select x;

            return day.Count();
        }
        public void GetAllLectures()
        {
            var tblLectures = DataAccess.GetContext().GetAllLectures();
            var tblStudents = DataAccess.GetContext().GetAllStudents();

            var output = tblLectures.Join(tblStudents,
                     lecture => lecture.UserId,
                     student => student.UserId,
                     (lecture, student) => new LectureReport(
                        lecture.UserId,
                        student.StudentNo,
                        lecture.LectureDate.ToString(),
                        "Yes",
                        lecture.ModuleCode)).ToList();
            lstReports.AddRange(output);
        }
    }
}

public class LectureReport() 
{
    public string userID { get; set; }
    public string stNumber { get; set; }
    public string date { get; set; }
    public string attendance { get; set; }
    public string module { get; set; }
    public double duration { get; set; }

    public LectureReport(string userID, string stNumber, string date, string attendance, string module): this()
    {
        this.userID = userID;
        this.stNumber = stNumber;
        this.date = date;
        this.attendance = attendance;
        this.module = module;
    }

    public void CalculateDuration()
    {
        //Add Logic
    }

    public int GetAttendance()
    {
        return DataAccess.GetContext().CalcDaysAttended(userID);
    }
}

