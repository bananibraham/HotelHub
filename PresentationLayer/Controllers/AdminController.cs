using BLLayer1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IDashboardBL _dashboardBL;

        public AdminController(IDashboardBL dashboardBL)
        {
            _dashboardBL = dashboardBL;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardBL.GetDashboardDataAsync();
            return View(data);
        }
    }
}
