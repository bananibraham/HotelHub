using HotelHub.Models.Entities;
using BLLayer1.Services;
using DataAccessLayer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly ICustomerService _customerService;
        private readonly ApplicationDbContext _context;

        public ReviewController(IReviewService reviewService, ICustomerService customerService, ApplicationDbContext context)
        {
            _reviewService = reviewService;
            _customerService = customerService;
            _context = context;
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
            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FirstName");
            ViewBag.Bookings = new SelectList(await _context.Bookings.ToListAsync(), "BookingId", "BookingId");
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
                    ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FirstName");
                    ViewBag.Bookings = new SelectList(await _context.Bookings.ToListAsync(), "BookingId", "BookingId");
                    return View(review);
                }

                await _reviewService.AddAsync(review);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FirstName");
            ViewBag.Bookings = new SelectList(await _context.Bookings.ToListAsync(), "BookingId", "BookingId");
            return View(review);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();

            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FirstName", review.CustomerId);
            ViewBag.Bookings = new SelectList(await _context.Bookings.ToListAsync(), "BookingId", "BookingId", review.BookingId);
            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (id != review.ReviewId) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    await _reviewService.UpdateAsync(review);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _reviewService.GetByIdAsync(id) == null)
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Customers = new SelectList(await _customerService.GetAllAsync(), "CustomerId", "FirstName", review.CustomerId);
            ViewBag.Bookings = new SelectList(await _context.Bookings.ToListAsync(), "BookingId", "BookingId", review.BookingId);
            return View(review);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reviewService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}