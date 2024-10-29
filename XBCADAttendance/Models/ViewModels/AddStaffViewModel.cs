using Microsoft.AspNetCore.Mvc.Rendering;

namespace XBCADAttendance.Models.ViewModels
{
    public class AddStaffViewModel
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string RoleID { get; set; }
        public string Name { get; set; }
        public string StaffNumber { get; set; }

        public List<TblRole> lstRoles = new List<TblRole>();

        public AddStaffViewModel()
        {
            lstRoles = DataAccess.GetAllRoles().Result;
        }

        public AddStaffViewModel(string role, string name, string staffNumber)
        {
            lstRoles = DataAccess.GetAllRoles().Result;

            Name = name;
            Role = role;

            RoleID = lstRoles.Where(x => x.RoleName == Role).Select(x => x.RoleId).FirstOrDefault();

            StaffNumber = staffNumber;
        }

        public IEnumerable<SelectListItem> GetRoles()
        {
            return lstRoles.Select(x => new SelectListItem { Value = x.RoleId, Text = x.RoleName }).ToList();

        }
    }
}
