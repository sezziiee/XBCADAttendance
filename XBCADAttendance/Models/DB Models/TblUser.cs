using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblUser
{
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<TblLecture> TblLectures { get; set; } = new List<TblLecture>();

    public virtual ICollection<TblStaff> TblStaffs { get; set; } = new List<TblStaff>();

    public virtual ICollection<TblStudent> TblStudents { get; set; } = new List<TblStudent>();
}
