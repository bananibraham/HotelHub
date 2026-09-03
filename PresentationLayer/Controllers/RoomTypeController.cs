using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
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
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(roomTypes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(roomType);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(RoomType roomType)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(roomType);
            }

            var result = await _roomTypeBL.CreateAsync(roomType);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to create Room Type.");
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(roomType);
            }

            TempData["Success"] = $"Room Type '{roomType.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(roomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, RoomType roomType)
        {
            if (id != roomType.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(roomType);
            }

            var result = await _roomTypeBL.UpdateAsync(roomType);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Failed to update Room Type.");
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(roomType);
            }

            TempData["Success"] = $"Room Type '{roomType.Name}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _roomTypeBL.GetByIdAsync(id);
            if (roomType == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(roomType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _roomTypeBL.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Room Type removed successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
