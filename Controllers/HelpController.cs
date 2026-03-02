using Microsoft.AspNetCore.Mvc;

namespace Alumni_Management_System.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
