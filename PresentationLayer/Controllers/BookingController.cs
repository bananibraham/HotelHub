using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly IBookingBL _bookingBL;
        private readonly ICustomerBL _customerBL;
        private readonly IRoomBL _roomBL;
        private readonly IRoomTypeBL _roomTypeBL;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(
            IBookingBL bookingBL,
            ICustomerBL customerBL,
            IRoomBL roomBL,
            IRoomTypeBL roomTypeBL,
            UserManager<IdentityUser> userManager)
        {
            _bookingBL = bookingBL;
            _customerBL = customerBL;
            _roomBL = roomBL;
            _roomTypeBL = roomTypeBL;
            _userManager = userManager;
        }

        // GET: /Booking (List bookings based on role)
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                var allBookings = await _bookingBL.GetAllAsync();
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(allBookings);
            }
            else
            {
                // Customer sees only their own bookings
                var customer = await GetOrCreateCurrentCustomerAsync();
                var customerBookings = customer != null
                    ? await _bookingBL.GetByCustomerIdAsync(customer.CustomerId)
                    : Enumerable.Empty<BookingVM>();

                return View("MyBookings", customerBookings);
            }
        }

        // GET: /Booking/AvailableRooms (Public)
        [AllowAnonymous]
        public async Task<IActionResult> AvailableRooms(DateTime? checkIn, DateTime? checkOut, int? roomTypeId, int? roomId, string? selection, int? guestCount)
        {
            var inDate = checkIn ?? DateTime.Today;
            if (inDate < DateTime.Today)
            {
                inDate = DateTime.Today;
            }

            var outDate = checkOut ?? inDate.AddDays(1);
            if (outDate <= inDate)
            {
                outDate = inDate.AddDays(1);
            }

            if (!string.IsNullOrWhiteSpace(selection))
            {
                if (string.Equals(selection, "all", StringComparison.OrdinalIgnoreCase))
                {
                    roomTypeId = null;
                    roomId = null;
                }
                else if (selection.StartsWith("type_", StringComparison.OrdinalIgnoreCase) && int.TryParse(selection[5..], out var parsedTypeId))
                {
                    roomTypeId = parsedTypeId;
                    roomId = null;
                }
                else if (selection.StartsWith("room_", StringComparison.OrdinalIgnoreCase) && int.TryParse(selection[5..], out var parsedRoomId))
                {
                    roomId = parsedRoomId;
                    roomTypeId = null;
                }
            }

            var availableRooms = await _roomBL.GetAvailableRoomsAsync(inDate, outDate, roomTypeId, roomId, guestCount);
            var roomTypes = await _roomTypeBL.GetAllAsync();
            var allRooms = await _roomBL.GetAllAsync();

            ViewBag.CheckIn = inDate;
            ViewBag.CheckOut = outDate;
            ViewBag.RoomTypeId = roomTypeId;
            ViewBag.RoomId = roomId;
            ViewBag.Selection = selection ?? (roomId.HasValue ? $"room_{roomId.Value}" : (roomTypeId.HasValue ? $"type_{roomTypeId.Value}" : "all"));
            ViewBag.GuestCount = guestCount;
            ViewBag.Nights = Math.Max(1, (outDate - inDate).Days);
            ViewBag.RoomTypes = new SelectList(roomTypes, "Id", "Name", roomTypeId);
            ViewBag.AllRooms = allRooms;

            return View(availableRooms);
        }

        // GET: /Booking/Checkout?roomId=5&checkIn=...&checkOut=...
        public async Task<IActionResult> Checkout(int roomId, DateTime? checkIn, DateTime? checkOut)
        {
            var room = await _roomBL.GetByIdAsync(roomId);
            if (room == null || !room.IsActive)
            {
                TempData["ErrorMessage"] = "The selected room is unavailable.";
                return RedirectToAction(nameof(AvailableRooms));
            }

            var inDate = checkIn ?? DateTime.Today;
            if (inDate < DateTime.Today)
            {
                inDate = DateTime.Today;
            }

            var outDate = checkOut ?? inDate.AddDays(1);
            if (outDate <= inDate)
            {
                outDate = inDate.AddDays(1);
            }

            bool isAvailable = await _roomBL.IsRoomAvailableAsync(roomId, inDate, outDate);
            if (!isAvailable)
            {
                TempData["ErrorMessage"] = $"Room {room.RoomNumber} is already booked or currently unavailable for the selected dates ({inDate:yyyy-MM-dd} to {outDate:yyyy-MM-dd}). Please choose another room or different dates.";
                return RedirectToAction(nameof(AvailableRooms), new { checkIn = inDate.ToString("yyyy-MM-dd"), checkOut = outDate.ToString("yyyy-MM-dd") });
            }

            int nights = Math.Max(1, (outDate - inDate).Days);
            decimal rate = room.RoomType?.PricePerNight ?? 1500m;
            decimal totalPrice = nights * rate;

            var customer = await GetOrCreateCurrentCustomerAsync();

            ViewBag.Room = room;
            ViewBag.Nights = nights;
            ViewBag.TotalPrice = totalPrice;

            var vm = new BookingVM
            {
                RoomId = roomId,
                RoomNumber = room.RoomNumber.ToString(),
                RoomType = room.RoomType?.Name ?? "Standard",
                CustomerId = customer.CustomerId,
                CustomerName = customer.FullName,
                CustomerEmail = customer.Email,
                CustomerPhone = customer.Phone,
                CheckInDate = inDate,
                CheckOutDate = outDate,
                TotalPrice = totalPrice,
                Status = "Confirmed"
            };

            return View(vm);
        }

        // POST: /Booking/ConfirmCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(BookingVM vm)
        {
            var customer = await GetOrCreateCurrentCustomerAsync();
            vm.CustomerId = customer.CustomerId;

            var room = await _roomBL.GetByIdAsync(vm.RoomId);
            if (room == null || !room.IsActive)
            {
                TempData["ErrorMessage"] = "The chosen room is no longer available.";
                return RedirectToAction(nameof(AvailableRooms));
            }

            bool isAvailable = await _roomBL.IsRoomAvailableAsync(vm.RoomId, vm.CheckInDate, vm.CheckOutDate);
            if (!isAvailable)
            {
                TempData["ErrorMessage"] = $"Room {room.RoomNumber} is already booked or currently unavailable for the selected dates ({vm.CheckInDate:yyyy-MM-dd} to {vm.CheckOutDate:yyyy-MM-dd}). Please select another room or choose different dates.";
                return RedirectToAction(nameof(AvailableRooms), new { checkIn = vm.CheckInDate.ToString("yyyy-MM-dd"), checkOut = vm.CheckOutDate.ToString("yyyy-MM-dd") });
            }

            int nights = Math.Max(1, (vm.CheckOutDate.Date - vm.CheckInDate.Date).Days);
            decimal rate = room.RoomType?.PricePerNight ?? 1500m;
            vm.TotalPrice = nights * rate;
            vm.Status = "Confirmed";

            int bookingId = await _bookingBL.CreateAndReturnIdAsync(vm);
            if (bookingId <= 0)
            {
                TempData["ErrorMessage"] = "Failed to create booking reservation. The room is already booked for the selected period.";
                return RedirectToAction(nameof(AvailableRooms), new { checkIn = vm.CheckInDate.ToString("yyyy-MM-dd"), checkOut = vm.CheckOutDate.ToString("yyyy-MM-dd") });
            }

            TempData["SuccessMessage"] = $"Reservation #{bookingId} created! Please complete payment.";
            return RedirectToAction("Checkout", "Payment", new { bookingId });
        }

        // GET: /Booking/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Customer check
            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetOrCreateCurrentCustomerAsync();
                if (booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            return View(booking);
        }

        // GET: /Booking/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetOrCreateCurrentCustomerAsync();
                if (booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(booking);
        }

        // GET: /Booking/Create (Staff Walk-in Booking)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await PopulateStaffDropdownsAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            var model = new BookingVM
            {
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1),
                Status = "Confirmed"
            };
            return View(model);
        }

        // POST: /Booking/Create (Staff Walk-in Booking)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(BookingVM vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateStaffDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(vm);
            }

            bool isAvailable = await _roomBL.IsRoomAvailableAsync(vm.RoomId, vm.CheckInDate, vm.CheckOutDate);
            if (!isAvailable)
            {
                ModelState.AddModelError("RoomId", $"The selected room is already booked or unavailable between {vm.CheckInDate:yyyy-MM-dd} and {vm.CheckOutDate:yyyy-MM-dd}.");
                await PopulateStaffDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(vm);
            }

            int bookingId = await _bookingBL.CreateAndReturnIdAsync(vm);
            if (bookingId <= 0)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the booking. The room may already be reserved.");
                await PopulateStaffDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Walk-in Booking #{bookingId} created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Booking/Edit/5 (Staff only)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            await PopulateStaffDropdownsAsync(booking.CustomerId, booking.RoomId, booking.Status);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(booking);
        }

        // POST: /Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id, BookingVM vm)
        {
            if (id != vm.BookingId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateStaffDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(vm);
            }

            bool isAvailable = await _roomBL.IsRoomAvailableAsync(vm.RoomId, vm.CheckInDate, vm.CheckOutDate, vm.BookingId);
            if (!isAvailable)
            {
                ModelState.AddModelError("RoomId", $"The selected room is already booked or unavailable between {vm.CheckInDate:yyyy-MM-dd} and {vm.CheckOutDate:yyyy-MM-dd}.");
                await PopulateStaffDropdownsAsync(vm.CustomerId, vm.RoomId, vm.Status);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
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
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetOrCreateCurrentCustomerAsync();
                if (booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }

                if (booking.CheckInDate.Date <= DateTime.Today)
                {
                    TempData["ErrorMessage"] = "Reservations starting today or in the past cannot be cancelled online.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var success = await _bookingBL.CancelAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = $"Booking #{id} cannot be cancelled in its current state.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Booking #{id} has been successfully cancelled.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Booking/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Confirm(int id)
        {
            var (success, message) = await _bookingBL.ConfirmAsync(id);
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

        // POST: /Booking/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
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
        [Authorize(Roles = "Admin,Receptionist")]
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

        // GET: /Booking/Delete/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookingBL.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(booking);
        }

        // POST: /Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        private async Task<CustomerVM> GetOrCreateCurrentCustomerAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var customers = await _customerBL.GetAllAsync();
            var customer = customers.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));

            if (customer != null)
            {
                return customer;
            }

            var user = await _userManager.FindByEmailAsync(email);
            var fullName = user?.UserName ?? "Valued Guest";
            var claimName = User.FindFirst(ClaimTypes.GivenName)?.Value;
            if (!string.IsNullOrWhiteSpace(claimName))
            {
                fullName = claimName;
            }

            var newCustomer = new CustomerVM
            {
                FullName = fullName,
                Email = email,
                Phone = user?.PhoneNumber ?? "01000000000",
                IsActive = true
            };

            await _customerBL.CreateAsync(newCustomer);
            var updatedCustomers = await _customerBL.GetAllAsync();
            return updatedCustomers.First(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        private async Task PopulateStaffDropdownsAsync(int? selectedCustomerId = null, int? selectedRoomId = null, string? selectedStatus = null)
        {
            var customers = await _customerBL.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "FullName", selectedCustomerId);

            var rooms = await _roomBL.GetAllAsync();
            var roomItems = rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"Room {r.RoomNumber} - {r.RoomType?.Name} ({r.RoomType?.PricePerNight:N0} EGP/nt)",
                Selected = selectedRoomId.HasValue && r.Id == selectedRoomId.Value
            }).ToList();

            ViewBag.Rooms = roomItems;

            var statuses = new[] { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            ViewBag.Statuses = new SelectList(statuses, selectedStatus ?? "Confirmed");
        }
    }
}
