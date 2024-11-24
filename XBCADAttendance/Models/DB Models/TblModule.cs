using System;
using System.Collections.Generic;

namespace XBCADAttendance.Models;

public partial class TblModule
{
    public string ModuleCode { get; set; } = null!;

    public string? ModuleName { get; set; }

    public virtual ICollection<TblStaffLecture> TblStaffLectures { get; set; } = new List<TblStaffLecture>();

    public virtual ICollection<TblUserModules> TblUserModules { get; set; } = new List<TblUserModules>();
}