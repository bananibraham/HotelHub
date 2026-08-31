using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceBL _invoiceBL;

        public InvoiceController(IInvoiceBL invoiceBL)
        {
            _invoiceBL = invoiceBL;
        }


        public async Task<IActionResult> Index()
        {
            var invoices =
                await _invoiceBL.GetAllAsync();

            return View(invoices);
        }


        public async Task<IActionResult> Details(int id)
        {
            Invoice? invoice =
                await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateVm invoiceVm)
        {
            if (!ModelState.IsValid)
            {
                return View(invoiceVm);
            }


            bool result =
                await _invoiceBL.CreateAsync(invoiceVm);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Invoice could not be created. Check the booking or invoice information.");

                return View(invoiceVm);
            }


            TempData["Success"] =
                "Invoice created successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Invoice? invoice =
                await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return View(invoice);
            }


            bool result =
                await _invoiceBL.UpdateAsync(invoice);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Invoice could not be updated.");

                return View(invoice);
            }


            TempData["Success"] =
                "Invoice updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Invoice? invoice =
                await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result =
                await _invoiceBL.DeleteAsync(id);


            if (!result)
            {
                return NotFound();
            }


            TempData["Success"] =
                "Invoice deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }

}
