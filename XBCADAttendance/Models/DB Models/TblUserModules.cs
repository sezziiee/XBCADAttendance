namespace XBCADAttendance.Models
{
    public class TblUserModules
    {
        public string ModuleCode { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public virtual TblModule TblModule { get; set; } = null!;

        public virtual TblUser TblUser { get; set; } = null!;
    }

}