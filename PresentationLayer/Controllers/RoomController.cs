using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer.Controllers
{


    [Authorize]
    public class RoomController : Controller
    {
        private readonly IRoomBL _roomBL;
        private readonly IRoomTypeBL _roomTypeBL;

        public RoomController(IRoomBL roomBL, IRoomTypeBL roomTypeBL)
        {
            _roomBL = roomBL;
            _roomTypeBL = roomTypeBL;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _roomBL.GetAllAsync();

            return View(rooms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);

            if (room == null)
                return NotFound();

            return View(room);
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await LoadRoomTypes();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomTypes(room.RoomTypeId);
                return View(room);
            }

            var result = await _roomBL.CreateAsync(room);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to create Room.");
                await LoadRoomTypes(room.RoomTypeId);
                return View(room);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);

            if (room == null)
                return NotFound();

            await LoadRoomTypes(room.RoomTypeId);

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadRoomTypes(room.RoomTypeId);
                return View(room);
            }

            var result = await _roomBL.UpdateAsync(room);

            if (!result)
            {
                ModelState.AddModelError("", "Failed to update Room.");
                await LoadRoomTypes(room.RoomTypeId);
                return View(room);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);

            if (room == null)
                return NotFound();

            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _roomBL.DeleteAsync(id);

            if (!result)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadRoomTypes(int? selectedRoomTypeId = null)
        {
            var roomTypes = await _roomTypeBL.GetAllAsync();

            ViewBag.RoomTypes = new SelectList(
                roomTypes,
                "Id",
                "Name",
                selectedRoomTypeId
            );
        }
    }
}

