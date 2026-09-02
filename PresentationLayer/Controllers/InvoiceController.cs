using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceBL _invoiceBL;

        public InvoiceController(IInvoiceBL invoiceBL)
        {
            _invoiceBL = invoiceBL;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceBL.GetAllAsync();

            return View(invoices);
        }

        public async Task<IActionResult> Details(int id)
        {
            Invoice? invoice = await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadBookingsAsync();

            return View(new InvoiceCreateVm
            {
                IssueDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateVm invoiceVm)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsAsync(invoiceVm.BookingId);
                return View(invoiceVm);
            }

            bool result = await _invoiceBL.CreateAsync(invoiceVm);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Invoice could not be created. Make sure the booking exists, the invoice does not already exist, and the payment amount is valid."
                );

                await LoadBookingsAsync(invoiceVm.BookingId);

                return View(invoiceVm);
            }

            TempData["Success"] = "Invoice created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Invoice? invoice = await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            await LoadBookingsAsync(invoice.BookingId);

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsAsync(invoice.BookingId);
                return View(invoice);
            }

            bool result = await _invoiceBL.UpdateAsync(invoice);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Invoice could not be updated. Make sure the booking exists and the payment total is valid."
                );

                await LoadBookingsAsync(invoice.BookingId);

                return View(invoice);
            }

            TempData["Success"] = "Invoice updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Invoice? invoice = await _invoiceBL.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result = await _invoiceBL.DeleteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Invoice deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBookingsAsync(int? selectedBookingId = null)
        {
            var bookings = await _invoiceBL.GetBookingsAsync();

            ViewBag.Bookings = bookings
                .Select(b => new SelectListItem
                {
                    Value = b.BookingId.ToString(),
                    Text =
                        $"Booking #{b.BookingId} | " +
                        $"{b.CheckInDate:dd MMM yyyy} → {b.CheckOutDate:dd MMM yyyy} | " +
                        $"Total: {b.TotalPrice:N2}",
                    Selected = selectedBookingId.HasValue &&
                               b.BookingId == selectedBookingId.Value
                })
                .ToList();
        }
    }
}