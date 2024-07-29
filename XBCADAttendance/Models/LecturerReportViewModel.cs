using Microsoft.Extensions.Configuration.UserSecrets;

namespace XBCADAttendance.Models
{
	public class LecturerReportViewModel
	{
		public string userID { get; set; }
		public string stNumber { get; set; }
		public string date { get; set; }
		public string attendance { get; set; }
		public string module { get; set; }
		public double duration { get; set; }
		public int totalDays { get; set; }

		public LecturerReportViewModel(string userId, string stNumber, string date, string attendance, int totalDays) 
		{
			this.userID = userId;
			this.stNumber = stNumber;
			this.date = date;
			this.attendance = attendance;
			this.totalDays = totalDays;
		}


    }
}
