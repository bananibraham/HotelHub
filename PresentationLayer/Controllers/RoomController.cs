using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
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
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(rooms);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(room);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadFormData();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(new Room { Status = "Available", ImageUrl = "/images/room-1.jpg" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Room room)
        {
            if (!await _roomBL.IsRoomNumberUniqueAsync(room.RoomNumber))
            {
                ModelState.AddModelError("RoomNumber", $"Room number {room.RoomNumber} already exists. Please choose a unique number.");
            }

            if (!ModelState.IsValid)
            {
                await LoadFormData(room.RoomTypeId, room.ImageUrl);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(room);
            }

            var result = await _roomBL.CreateAsync(room);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to create Room in database.");
                await LoadFormData(room.RoomTypeId, room.ImageUrl);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(room);
            }

            TempData["Success"] = $"Room {room.RoomNumber} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            await LoadFormData(room.RoomTypeId, room.ImageUrl);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
            {
                return BadRequest();
            }

            if (!await _roomBL.IsRoomNumberUniqueAsync(room.RoomNumber, room.Id))
            {
                ModelState.AddModelError("RoomNumber", $"Room number {room.RoomNumber} is already assigned to another room.");
            }

            if (!ModelState.IsValid)
            {
                await LoadFormData(room.RoomTypeId, room.ImageUrl);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(room);
            }

            var result = await _roomBL.UpdateAsync(room);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update Room.");
                await LoadFormData(room.RoomTypeId, room.ImageUrl);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(room);
            }

            TempData["Success"] = $"Room {room.RoomNumber} updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomBL.GetByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _roomBL.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Room removed successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFormData(int? selectedRoomTypeId = null, string? selectedImage = null)
        {
            var roomTypes = await _roomTypeBL.GetAllAsync();
            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", selectedRoomTypeId);

            ViewBag.AvailableImages = new List<SelectListItem>
            {
                new SelectListItem { Value = "/images/room-1.jpg", Text = "Deluxe Suite (room-1.jpg)", Selected = selectedImage == "/images/room-1.jpg" },
                new SelectListItem { Value = "/images/room-2.jpg", Text = "Family Room (room-2.jpg)", Selected = selectedImage == "/images/room-2.jpg" },
                new SelectListItem { Value = "/images/room-3.jpg", Text = "Deluxe Ocean Room (room-3.jpg)", Selected = selectedImage == "/images/room-3.jpg" },
                new SelectListItem { Value = "/images/room-4.jpg", Text = "Classic Garden Room (room-4.jpg)", Selected = selectedImage == "/images/room-4.jpg" },
                new SelectListItem { Value = "/images/room-5.jpg", Text = "Superior Terrace (room-5.jpg)", Selected = selectedImage == "/images/room-5.jpg" },
                new SelectListItem { Value = "/images/room-6.jpg", Text = "Luxury Penthouse (room-6.jpg)", Selected = selectedImage == "/images/room-6.jpg" }
            };

            var statuses = new List<string> { "Available", "Occupied", "UnderMaintenance" };
            ViewBag.Statuses = new SelectList(statuses);
        }
    }
}
