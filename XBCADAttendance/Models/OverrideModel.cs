namespace XBCADAttendance.Models
{
    public class OverrideModel
    {
        // Lecture Table
        public string? LectureId { get; set; }
        public string? LectureUserId { get; set; }
        public DateOnly? LectureDate { get; set; }
        public string? ClassroomCode { get; set; }
        public TimeOnly? ScanIn { get; set; }
        public TimeOnly? ScanOut { get; set; }
        public string? ModuleCode { get; set; } // Module code is already part of Lecture Table

        // TblModule Table
        public string? ModuleName { get; set; }

        // TblRole Table
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }

        // TblStaff Table
        public string? StaffId { get; set; }
        public string? StaffUserId { get; set; }
        public string? StaffRoleId { get; set; }

        // TblStudent Table
        public string? StudentNo { get; set; }
        public string? StudentUserId { get; set; }

        // TblUser Table
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
