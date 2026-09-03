using BLLayer1.Interfaces;
using HotelHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HotelHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRoomBL _roomBL;
        private readonly IRoomTypeBL _roomTypeBL;

        public HomeController(IRoomBL roomBL, IRoomTypeBL roomTypeBL)
        {
            _roomBL = roomBL;
            _roomTypeBL = roomTypeBL;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomBL.GetAllAsync();
            var roomTypes = await _roomTypeBL.GetAllAsync();

            ViewBag.RoomTypes = roomTypes;
            return View(rooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
