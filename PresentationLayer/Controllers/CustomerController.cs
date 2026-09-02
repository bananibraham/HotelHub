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

        // GET: Customer
        public async Task<IActionResult> Index(string? search)
        {
            var customers = await _customerBL.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                customers = customers.Where(c => 
                    c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Phone.Contains(search) ||
                    c.NationalId.Contains(search));
                
                ViewData["CurrentFilter"] = search;
            }
            
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View(new CustomerVM { IsActive = true });
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerVM customerVm)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerBL.CreateAsync(customerVm);
                
                if (result)
                {
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("", "Failed to create customer. Email or National ID may already exist.");
            }
            
            return View(customerVm);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerVM customerVm)
        {
            if (id != customerVm.CustomerId) return BadRequest();

            if (ModelState.IsValid)
            {
                var result = await _customerBL.UpdateAsync(customerVm);
                
                if (result)
                {
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("", "Failed to update customer. Email or National ID may already exist.");
            }
            
            return View(customerVm);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _customerBL.DeleteAsync(id);
            
            if (result)
            {
                TempData["Success"] = "Customer deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete customer.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}