using BLLayer1.Interfaces;
using BLLayer1.MockData;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingBL _bookingBL;
        private readonly ICustomerBL _customerBL;

        public BookingController(IBookingBL bookingBL, ICustomerBL customerBL)
        {
            _bookingBL = bookingBL;
            _customerBL = customerBL;
        }

        // GET: /Booking
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingBL.GetAllAsync();
            return View(bookings);
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: /Booking/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            var model = new BookingVM
            {
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                Status = "Pending"
            };
            return View(model);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingVM vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                return View(vm);
            }

            var result = await _bookingBL.CreateAsync(vm);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the booking.");
                await PopulateDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                return View(vm);
            }

            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(booking.CustomerId, booking.RoomId, booking.Status);
            return View(booking);
        }

        // POST: /Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingVM vm)
        {
            if (id != vm.BookingId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                return View(vm);
            }

            var success = await _bookingBL.UpdateAsync(vm);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Booking updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var success = await _bookingBL.CancelAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = $"Booking #{id} could not be cancelled.";
                return NotFound();
            }

            TempData["SuccessMessage"] = $"Booking #{id} has been cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var (success, message) = await _bookingBL.CheckInAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            var (success, message) = await _bookingBL.CheckOutAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: /Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _bookingBL.DeleteAsync(id);
            if (!success)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Booking deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? selectedCustomerId = null, int? selectedRoomId = null, string? selectedStatus = null)
        {
            var customers = await _customerBL.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "FullName", selectedCustomerId);

            var rooms = MockRoomData.GetMockRooms();
            ViewBag.Rooms = new SelectList(rooms, "RoomId", "DisplayName", selectedRoomId);

            var statuses = new[] { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            ViewBag.Statuses = new SelectList(statuses, selectedStatus ?? "Pending");
        }
    }
}
