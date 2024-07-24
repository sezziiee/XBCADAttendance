using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;

namespace XBCADAttendance.Controllers
{
    public class EditController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Update(OverrideModel model)
        {
            DataAccess.GetContext().Update
            return View();
        }

        public IActionResult Delete() 
        { 
            return View();
        }
}
}
