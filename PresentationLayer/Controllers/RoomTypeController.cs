using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeBL _roomTypeBL;

        public RoomTypeController(IRoomTypeBL roomTypeBL)
        {
            _roomTypeBL = roomTypeBL;
        }


        public async Task<IActionResult> Index()
        {
            var roomTypes = await _roomTypeBL.GetAllAsync();

            return View(roomTypes);
        }


        public async Task<IActionResult> Details(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);

            if (roomType == null)
                return NotFound();

            return View(roomType);
        }


        [Authorize(Roles = "Admin,Receptionist")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(RoomType roomType)
        {
            if (!ModelState.IsValid)
                return View(roomType);

            await _roomTypeBL.AddAsync(roomType);

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);

            if (roomType == null)
                return NotFound();

            return View(roomType);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id, RoomType roomType)
        {
            if (id != roomType.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(roomType);

            _roomTypeBL.Update(roomType);
            await _roomTypeBL.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);

            if (roomType == null)
                return NotFound();

            return View(roomType);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomTypeBL.DeleteAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}
