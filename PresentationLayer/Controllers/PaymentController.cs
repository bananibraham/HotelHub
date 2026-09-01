using BLLayer1.Interfaces;
using BLLayer1.BLogic;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentBL _paymentBL;

        public PaymentController(IPaymentBL paymentBL)
        {
            _paymentBL = paymentBL;
        }


        public async Task<IActionResult> Index()
        {
            var payments =
                await _paymentBL.GetAllAsync();

            return View(payments);
        }


        public async Task<IActionResult> Details(int id)
        {
            Payment? payment =
                await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateVm paymentVm)
        {
            if (!ModelState.IsValid)
            {
                return View(paymentVm);
            }


            bool result =
                await _paymentBL.CreateAsync(paymentVm);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Payment could not be created. Check the booking and payment amount.");

                return View(paymentVm);
            }


            TempData["Success"] =
                "Payment created successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Payment? payment =
                await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                return View(payment);
            }


            bool result =
                await _paymentBL.UpdateAsync(payment);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Payment could not be updated. Check the payment amount.");

                return View(payment);
            }


            TempData["Success"] =
                "Payment updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Payment? payment =
                await _paymentBL.GetByIdAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result =
                await _paymentBL.DeleteAsync(id);


            if (!result)
            {
                return NotFound();
            }


            TempData["Success"] =
                "Payment deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }

}
