using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewBL _reviewBL;
        private readonly ICustomerBL _customerBL;

        public ReviewController(IReviewBL reviewBL, ICustomerBL customerBL)
        {
            _reviewBL = reviewBL;
            _customerBL = customerBL;
        }

        // GET: Review
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewBL.GetAllAsync();
            return View(reviews);
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        // GET: Review/Create
        public async Task<IActionResult> Create()
        {
            var vm = new ReviewVM
            {
                Customers = await _customerBL.GetActiveCustomersAsSelectListAsync(),
                Bookings = new List<SelectListItem>() // Empty initially, will be populated via AJAX
            };
            return View(vm);
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewVM reviewVm)
        {
            if (ModelState.IsValid)
            {
                var result = await _reviewBL.CreateAsync(reviewVm);
                
                if (result)
                {
                    TempData["Success"] = "Review added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("", "Failed to add review. Please ensure the booking belongs to the selected customer.");
            }
            
            reviewVm.Customers = await _customerBL.GetActiveCustomersAsSelectListAsync();
            reviewVm.Bookings = reviewVm.CustomerId > 0 
                ? await _reviewBL.GetBookingsByCustomerAsSelectListAsync(reviewVm.CustomerId)
                : new List<SelectListItem>();
            
            return View(reviewVm);
        }

        // GET: Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();

            review.Customers = await _customerBL.GetActiveCustomersAsSelectListAsync();
            review.Bookings = review.CustomerId > 0 
                ? await _reviewBL.GetBookingsByCustomerAsSelectListAsync(review.CustomerId, review.BookingId)
                : new List<SelectListItem>();
            
            return View(review);
        }

        // POST: Review/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewVM reviewVm)
        {
            if (id != reviewVm.ReviewId) return BadRequest();

            if (ModelState.IsValid)
            {
                var result = await _reviewBL.UpdateAsync(reviewVm);
                
                if (result)
                {
                    TempData["Success"] = "Review updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("", "Failed to update review. Please ensure the booking belongs to the selected customer.");
            }
            
            reviewVm.Customers = await _customerBL.GetActiveCustomersAsSelectListAsync();
            reviewVm.Bookings = reviewVm.CustomerId > 0 
                ? await _reviewBL.GetBookingsByCustomerAsSelectListAsync(reviewVm.CustomerId, reviewVm.BookingId)
                : new List<SelectListItem>();
            
            return View(reviewVm);
        }

        // GET: Review/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _reviewBL.DeleteAsync(id);
            
            if (result)
            {
                TempData["Success"] = "Review deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete review.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // AJAX Endpoint: Get bookings for selected customer
        [HttpGet]
        public async Task<IActionResult> GetBookingsByCustomer(int customerId)
        {
            if (customerId <= 0)
            {
                return Json(new List<SelectListItem>());
            }

            var bookings = await _reviewBL.GetBookingsByCustomerAsSelectListAsync(customerId);
            return Json(bookings);
        }
    }
}