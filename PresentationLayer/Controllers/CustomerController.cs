using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class CustomerController : Controller
    {
        private readonly ICustomerBL _customerBL;

        public CustomerController(ICustomerBL customerBL)
        {
            _customerBL = customerBL;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerBL.GetAllAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(customer);
        }

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
        public async Task<IActionResult> Create(CustomerVM customerVm)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(customerVm);
            }

            await _customerBL.CreateAsync(customerVm);
            TempData["Success"] = $"Guest profile for '{customerVm.FullName}' created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerVM customerVm)
        {
            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(customerVm);
            }

            await _customerBL.UpdateAsync(customerVm);
            TempData["Success"] = $"Profile for '{customerVm.FullName}' updated.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _customerBL.GetByIdAsync(id);
            if (customer == null) return NotFound();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerBL.DeleteAsync(id);
            TempData["Success"] = "Customer profile deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}