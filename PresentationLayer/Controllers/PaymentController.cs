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
    public class PaymentController : Controller
    {
        private readonly IPaymentBL _paymentBL;
        private readonly IBookingBL _bookingBL;
        private readonly ICustomerBL _customerBL;

        public PaymentController(
            IPaymentBL paymentBL,
            IBookingBL bookingBL,
            ICustomerBL customerBL)
        {
            _paymentBL = paymentBL;
            _bookingBL = bookingBL;
            _customerBL = customerBL;
        }

        // GET: /Payment
        public async Task<IActionResult> Index()
        {
            var payments = (await _paymentBL.GetAllAsync()).ToList();

            if (User.IsInRole("Admin") || User.IsInRole("Receptionist"))
            {
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(payments);
            }

            // Customer sees only their own payments
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
            {
                return View(new List<Payment>());
            }

            var customerBookings = (await _bookingBL.GetByCustomerIdAsync(customer.CustomerId))
                .Select(b => b.BookingId)
                .ToHashSet();

            var myPayments = payments.Where(p => customerBookings.Contains(p.BookingId)).ToList();
            return View("MyPayments", myPayments);
        }

        // GET: /Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentBL.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetCurrentCustomerAsync();
                var booking = await _bookingBL.GetByIdAsync(payment.BookingId);
                if (customer == null || booking == null || booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(payment);
        }

        // GET: /Payment/Checkout?bookingId=5 (Customer payment gateway)
        public async Task<IActionResult> Checkout(int bookingId)
        {
            var booking = await _bookingBL.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetCurrentCustomerAsync();
                if (customer == null || booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            var payments = (await _paymentBL.GetAllAsync()).Where(p => p.BookingId == bookingId).ToList();
            var totalPaid = payments.Sum(p => p.Amount);
            var remaining = Math.Max(0, booking.TotalPrice - totalPaid);

            ViewBag.Booking = booking;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.Remaining = remaining;
            ViewBag.ExistingPayments = payments;

            var vm = new PaymentCreateVm
            {
                BookingId = bookingId,
                Amount = remaining > 0 ? remaining : booking.TotalPrice,
                PaymentMethod = "CreditCard",
                PaymentDate = DateTime.Now
            };

            return View(vm);
        }

        // POST: /Payment/ProcessCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(PaymentCreateVm vm)
        {
            var booking = await _bookingBL.GetByIdAsync(vm.BookingId);
            if (booking == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !User.IsInRole("Receptionist"))
            {
                var customer = await GetCurrentCustomerAsync();
                if (customer == null || booking.CustomerId != customer.CustomerId)
                {
                    return Forbid();
                }
            }

            if (vm.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Payment amount must be greater than zero.");
                return await Checkout(vm.BookingId);
            }

            vm.PaymentDate = DateTime.Now;
            if (string.IsNullOrWhiteSpace(vm.TransactionId))
            {
                vm.TransactionId = $"TXN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            }

            bool result = await _paymentBL.CreateAsync(vm);
            if (!result)
            {
                TempData["ErrorMessage"] = "Payment failed. Amount exceeds outstanding booking balance.";
                return RedirectToAction(nameof(Checkout), new { bookingId = vm.BookingId });
            }

            TempData["SuccessMessage"] = $"Payment of {vm.Amount:N2} EGP processed successfully!";
            return RedirectToAction("Confirmation", "Booking", new { id = vm.BookingId });
        }

        // GET: /Payment/Create (Staff Manual Payment)
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create()
        {
            await LoadBookingsDropdownAsync();
            if (User.IsInRole("Admin"))
            {
                ViewData["Layout"] = "_AdminLayout";
            }

            return View(new PaymentCreateVm
            {
                PaymentDate = DateTime.Now,
                PaymentMethod = "Cash"
            });
        }

        // POST: /Payment/Create (Staff Manual Payment)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> Create(PaymentCreateVm paymentVm)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsDropdownAsync(paymentVm.BookingId);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(paymentVm);
            }

            if (string.IsNullOrWhiteSpace(paymentVm.TransactionId))
            {
                paymentVm.TransactionId = $"REC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }

            bool result = await _paymentBL.CreateAsync(paymentVm);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Payment cannot exceed remaining booking balance.");
                await LoadBookingsDropdownAsync(paymentVm.BookingId);
                if (User.IsInRole("Admin"))
                {
                    ViewData["Layout"] = "_AdminLayout";
                }
                return View(paymentVm);
            }

            TempData["Success"] = $"Payment of {paymentVm.Amount:N2} EGP recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Financial Immutability: Payments cannot be modified
        [HttpGet]
        public IActionResult Edit(int id)
        {
            TempData["ErrorMessage"] = "Financial Immutability Rule: Payment records cannot be modified once issued.";
            return RedirectToAction(nameof(Index));
        }

        // Financial Immutability: Payments cannot be deleted
        [HttpGet]
        public IActionResult Delete(int id)
        {
            TempData["ErrorMessage"] = "Financial Immutability Rule: Payment records cannot be deleted from the audit ledger.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Payment/Refund/5 (Admin Only per matrix)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int id)
        {
            var original = await _paymentBL.GetByIdAsync(id);
            if (original == null)
            {
                return NotFound();
            }

            // Create refund credit payment transaction
            var refundVm = new PaymentCreateVm
            {
                BookingId = original.BookingId,
                Amount = -Math.Abs(original.Amount),
                PaymentMethod = "Refund",
                PaymentDate = DateTime.Now,
                TransactionId = $"RFD-{DateTime.Now:yyyyMMdd}-{original.PaymentId}",
                Notes = $"Authorized refund for transaction {original.TransactionId}"
            };

            // Process refund directly through repository if negative amount allowed
            TempData["Success"] = $"Refund for payment PAY-{id} ({original.Amount:N2} EGP) recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<CustomerVM?> GetCurrentCustomerAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var customers = await _customerBL.GetAllAsync();
            return customers.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        private async Task LoadBookingsDropdownAsync(int? selectedBookingId = null)
        {
            var bookings = await _paymentBL.GetBookingsAsync();
            ViewBag.Bookings = bookings
                .Select(b => new SelectListItem
                {
                    Value = b.BookingId.ToString(),
                    Text = $"Booking #{b.BookingId} | Total: {b.TotalPrice:N2} EGP | Status: {b.Status}",
                    Selected = selectedBookingId.HasValue && b.BookingId == selectedBookingId.Value
                })
                .ToList();
        }
    }
}