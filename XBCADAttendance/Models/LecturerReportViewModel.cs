namespace XBCADAttendance.Models
{
	public class LecturerReportViewModel
	{
		public string stNumber { get; set; }
		public string date { get; set; }
		public string attendance { get; set; }
		public string module { get; set; }
		public double duration { get; set; }
		public int totalDays { get; set; }

		public LecturerReportViewModel(string stNumber, string date, string attendance, int totalDays) 
		{
			this.stNumber = stNumber;
			this.date = date;
			this.attendance = attendance;
			this.totalDays = totalDays;
		}
    }
}
