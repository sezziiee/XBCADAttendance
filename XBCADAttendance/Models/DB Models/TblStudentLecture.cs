using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblStudentLecture
{
    public string LectureId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateOnly LectureDate { get; set; }

    public string ClassroomCode { get; set; } = null!;

    public TimeOnly ScanIn { get; set; }

    public TimeOnly? ScanOut { get; set; }

    public string ModuleCode { get; set; } = null!;

    public virtual TblStaffLecture Lecture { get; set; } = null!;

    public virtual TblModule ModuleCodeNavigation { get; set; } = null!;

    public virtual TblStudent User { get; set; } = null!;
}
