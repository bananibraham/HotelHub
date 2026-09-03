using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    public class ReceptionistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
