using HotelHub.Models.Entities;
using HotelHub.Repository.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelHub.Web.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ICustomerService _customerService;

        public ReviewController(IReviewService reviewService, ICustomerService customerService)
        {
            _reviewService = reviewService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewService.GetAllAsync();
            return View(reviews);
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FullName");
            // ViewBag.Bookings = new SelectList(await _bookingService.GetAllAsync(), "BookingId", "BookingNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            if (ModelState.IsValid)
            {
                if (await _reviewService.IsBookingReviewedAsync(review.BookingId))
                {
                    ModelState.AddModelError("BookingId", "This booking already has a review.");
                    ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FullName");
                    return View(review);
                }

                await _reviewService.AddAsync(review);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FullName");
            return View(review);
        }

        // Edit and Delete actions follow the same pattern as CustomerController
        // I recommend adding them using Scaffolding and replacing context with services.
    }
}