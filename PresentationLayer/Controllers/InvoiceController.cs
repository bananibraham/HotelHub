using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceBL _invoiceBL;
        private readonly IBookingBL _bookingBL;
        private readonly ICustomerBL _customerBL;
        private readonly IPaymentBL _paymentBL;

        public InvoiceController(
            IInvoiceBL invoiceBL,
            IBookingBL bookingBL,
            ICustomerBL customerBL,
            IPaymentBL paymentBL)
        {
            _invoiceBL = invoiceBL;
            _bookingBL = bookingBL;
            _customerBL = customerBL;
            _paymentBL = paymentBL;
        }

        // GET: /Invoice
        public async Task<IActionResult> Index()
        {
            var invoices = (await _invoiceBL.GetAllAsync()).ToList();

            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(invoices);
            }

            // Customer sees only invoices for their bookings
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
            {
                return View(new List<Invoice>());
            }

            var customerBookings = (await _bookingBL.GetByCustomerIdAsync(customer.CustomerId))
                .Select(b => b.BookingId)
                .ToHashSet();

            var myInvoices = invoices.Where(i => customerBookings.Contains(i.BookingId)).ToList();
            return View("MyInvoices", myInvoices);
        }

        // GET: /Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceBL.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetCurrentCustomerAsync();
                var booking = await _bookingBL.GetByIdAsync(invoice.BookingId);
                if (customer == null || booking == null || booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(invoice);
        }

        // GET: /Invoice/Create (Staff only)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await LoadBookingsAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(new InvoiceCreateVm
            {
                IssueDate = DateTime.Now
            });
        }

        // POST: /Invoice/Create (Staff only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(InvoiceCreateVm invoiceVm)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsAsync(invoiceVm.BookingId);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(invoiceVm);
            }

            bool result = await _invoiceBL.CreateAsync(invoiceVm);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Invoice could not be created. An invoice may already exist for this booking.");
                await LoadBookingsAsync(invoiceVm.BookingId);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(invoiceVm);
            }

            TempData["Success"] = "Invoice created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Invoice/Edit/5 (Update Invoice / Add Room Services - Staff only)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _invoiceBL.GetByIdAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            await LoadBookingsAsync(invoice.BookingId);
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(invoice);
        }

        // POST: /Invoice/Edit/5 (Update Invoice / Add Room Services - Staff only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Edit(Invoice invoice)
        {
            // Per the matrix: Edit Paid Amount inside Invoice is forbidden (it is computed strictly from real payments)
            var payments = await _paymentBL.GetAllAsync();
            decimal actualPaid = payments.Where(p => p.BookingId == invoice.BookingId).Sum(p => p.Amount);
            invoice.PaidAmount = actualPaid;
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - actualPaid);

            bool result = await _invoiceBL.UpdateAsync(invoice);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Invoice could not be updated.");
                await LoadBookingsAsync(invoice.BookingId);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(invoice);
            }

            TempData["Success"] = "Invoice updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Deletion forbidden per matrix
        [HttpGet]
        public IActionResult Delete(int id)
        {
            TempData["ErrorMessage"] = "Accounting Rule: Invoices cannot be deleted from the system.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<CustomerVM?> GetCurrentCustomerAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var customers = await _customerBL.GetAllAsync();
            return customers.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        private async Task LoadBookingsAsync(int? selectedBookingId = null)
        {
            var bookings = await _invoiceBL.GetBookingsAsync();
            ViewBag.Bookings = bookings
                .Select(b => new SelectListItem
                {
                    Value = b.BookingId.ToString(),
                    Text = $"Booking #{b.BookingId} | Total: {b.TotalPrice:N2} EGP",
                    Selected = selectedBookingId.HasValue && b.BookingId == selectedBookingId.Value
                })
                .ToList();
        }
    }
}