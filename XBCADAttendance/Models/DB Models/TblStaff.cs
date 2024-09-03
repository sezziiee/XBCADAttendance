using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblStaff
{
    public string UserId { get; set; } = null!;

    public string StaffId { get; set; } = null!;

    public string? RoleId { get; set; }

    public virtual TblRole? Role { get; set; }

    public virtual ICollection<TblStaffLecture> TblStaffLectures { get; set; } = new List<TblStaffLecture>();

    public virtual TblUser User { get; set; } = null!;
}
