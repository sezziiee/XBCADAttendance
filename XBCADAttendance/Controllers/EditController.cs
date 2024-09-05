using Microsoft.AspNetCore.Mvc;
using XBCADAttendance.Models;
using XBCADAttendance.Models.ViewModels;

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

            return View();
        }

        public IActionResult Delete() 
        { 
            return View();
        }
}
}
