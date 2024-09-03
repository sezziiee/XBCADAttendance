using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblStudent
{
    public string StudentNo { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public virtual ICollection<TblStudentLecture> TblStudentLectures { get; set; } = new List<TblStudentLecture>();

    public virtual TblUser User { get; set; } = null!;
}
