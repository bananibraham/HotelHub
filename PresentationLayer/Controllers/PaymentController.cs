using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PresentationLayer.Controllers
{
    [Authorize(Roles = "Admin,Receptionist")]
    public class PaymentController : Controller
    {
        private readonly IPaymentBL _paymentBL;

        public PaymentController(IPaymentBL paymentBL)
        {
            _paymentBL = paymentBL;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _paymentBL.GetAllAsync();

            return View(payments);
        }

        public async Task<IActionResult> Details(int id)
        {
            Payment? payment = await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadBookingsAsync();

            return View(new PaymentCreateVm
            {
                PaymentDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateVm paymentVm)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsAsync(paymentVm.BookingId);
                return View(paymentVm);
            }

            bool result = await _paymentBL.CreateAsync(paymentVm);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Payment could not be created. Make sure the booking exists and the total payments do not exceed the booking price."
                );

                await LoadBookingsAsync(paymentVm.BookingId);

                return View(paymentVm);
            }

            TempData["Success"] = "Payment created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Payment? payment = await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            await LoadBookingsAsync(payment.BookingId);

            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                await LoadBookingsAsync(payment.BookingId);
                return View(payment);
            }

            bool result = await _paymentBL.UpdateAsync(payment);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Payment could not be updated. Make sure the booking exists and the total payments do not exceed the booking price."
                );

                await LoadBookingsAsync(payment.BookingId);

                return View(payment);
            }

            TempData["Success"] = "Payment updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Payment? payment = await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result = await _paymentBL.DeleteAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Payment deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadBookingsAsync(int? selectedBookingId = null)
        {
            var bookings = await _paymentBL.GetBookingsAsync();

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