using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerBL _customerBL;

        public CustomerController(ICustomerBL customerBL)
        {
            _customerBL = customerBL;
        }

        // 1. List (Index)
        public async Task<IActionResult> Index()
        {
            var customers = await _customerBL.GetAllAsync();
            return View(customers);
        }

        // 2. Details
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // 3. Create (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerVM customerVm)
        {
            if (ModelState.IsValid)
            {
                await _customerBL.CreateAsync(customerVm);
                return RedirectToAction(nameof(Index));
            }
            return View(customerVm);
        }

        // 4. Edit (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // 4. Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerVM customerVm)
        {
            if (id != customerVm.CustomerId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _customerBL.UpdateAsync(customerVm);
                return RedirectToAction(nameof(Index));
            }
            return View(customerVm);
        }

        // 5. Delete (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // 5. Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerBL.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}