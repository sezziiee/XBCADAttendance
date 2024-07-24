using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblRole
{
    public string RoleId { get; set; } = null!;

    public string? RoleName { get; set; }

    public virtual ICollection<TblStaff> TblStaffs { get; set; } = new List<TblStaff>();
}
