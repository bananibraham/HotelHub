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
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewVM reviewVm)
        {
            if (ModelState.IsValid)
            {
                await _reviewBL.CreateAsync(reviewVm);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(reviewVm.CustomerId);
            return View(reviewVm);
        }

        // GET: Review/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _reviewBL.GetByIdAsync(id);
            if (review == null) return NotFound();

            await PopulateDropdownsAsync(review.CustomerId);
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
                await _reviewBL.UpdateAsync(reviewVm);
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(reviewVm.CustomerId);
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
            await _reviewBL.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdownsAsync(int? selectedCustomerId = null)
        {
            var customers = await _customerBL.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustomerId", "FullName", selectedCustomerId);
        }
    }
}