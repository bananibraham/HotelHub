using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewBL _reviewBL;
        private readonly ICustomerBL _customerBL;
        private readonly IBookingBL _bookingBL;

        public ReviewController(IReviewBL reviewBL, ICustomerBL customerBL, IBookingBL bookingBL)
        {
            _reviewBL = reviewBL;
            _customerBL = customerBL;
            _bookingBL = bookingBL;
        }

        // GET: Review
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewBL.GetAllAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(reviews);
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(review);
        }

        // GET: Review/Create
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(new ReviewVM { Rating = 5 });
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(ReviewVM reviewVm)
        {
            if (reviewVm.CustomerId <= 0 && reviewVm.BookingId.HasValue)
            {
                var booking = await _bookingBL.GetByIdAsync(reviewVm.BookingId.Value);
                if (booking != null)
                {
                    reviewVm.CustomerId = booking.CustomerId;
                    ModelState.Remove(nameof(reviewVm.CustomerId));
                }
            }

            if (ModelState.IsValid)
            {
                await _reviewBL.CreateAsync(reviewVm);
                TempData["Success"] = "Guest review recorded successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(reviewVm.CustomerId, reviewVm.BookingId);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(reviewVm);
        }

        // GET: Review/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();

            await PopulateDropdownsAsync(review.CustomerId, review.BookingId);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(review);
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ReviewVM reviewVm)
        {
            if (id != reviewVm.ReviewId) return BadRequest();

            if (reviewVm.CustomerId <= 0 && reviewVm.BookingId.HasValue)
            {
                var booking = await _bookingBL.GetByIdAsync(reviewVm.BookingId.Value);
                if (booking != null)
                {
                    reviewVm.CustomerId = booking.CustomerId;
                    ModelState.Remove(nameof(reviewVm.CustomerId));
                }
            }

            if (ModelState.IsValid)
            {
                await _reviewBL.UpdateAsync(reviewVm);
                TempData["Success"] = "Review updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(reviewVm.CustomerId, reviewVm.BookingId);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(reviewVm);
        }

        // GET: Review/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(review);
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reviewBL.DeleteAsync(id);
            TempData["Success"] = "Review removed from system.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? selectedCustomerId = null, int? selectedBookingId = null)
        {
            var customers = await _customerBL.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "FullName", selectedCustomerId);

            var bookings = await _bookingBL.GetAllAsync();
            var bookingItems = bookings.Select(b => new SelectListItem
            {
                Value = b.BookingId.ToString(),
                Text = $"Booking #{b.BookingId} — {b.CustomerName} ({b.RoomType} Room {b.RoomNumber})",
                Selected = selectedBookingId.HasValue && b.BookingId == selectedBookingId.Value
            }).ToList();

            ViewBag.Bookings = bookingItems;
        }
    }
}