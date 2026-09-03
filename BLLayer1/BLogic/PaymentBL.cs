using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.BLogic
{
    public class PaymentBL : IPaymentBL
    {
        private readonly IBasicOperation<Payment> _paymentRepository;
        private readonly IBasicOperation<Booking> _bookingRepository;
        private readonly IBasicOperation<Invoice> _invoiceRepository;

        public PaymentBL(
            IBasicOperation<Payment> paymentRepository,
            IBasicOperation<Booking> bookingRepository,
            IBasicOperation<Invoice> invoiceRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<bool> CreateAsync(PaymentCreateVm paymentVm)
        {
            if (paymentVm.Amount <= 0)
            {
                return false;
            }

            Booking? booking =
                await _bookingRepository.GetByIdAsync(paymentVm.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments =
                await _paymentRepository.GetAllAsync();

            decimal totalPaid = payments
                .Where(p => p.BookingId == paymentVm.BookingId)
                .Sum(p => p.Amount);

            decimal newTotalPaid = totalPaid + paymentVm.Amount;

            if (newTotalPaid > booking.TotalPrice)
            {
                return false;
            }

            Payment payment = new Payment
            {
                BookingId = paymentVm.BookingId,
                Amount = paymentVm.Amount,
                PaymentMethod = paymentVm.PaymentMethod,
                PaymentDate = paymentVm.PaymentDate,
                TransactionId = paymentVm.TransactionId,
                Notes = paymentVm.Notes,
                CreatedAt = DateTime.Now
            };

            await _paymentRepository.AddAsync(payment);

            int rowsAffected =
                await _paymentRepository.SaveChangesAsync();

            if (rowsAffected <= 0)
            {
                return false;
            }

            await RecalculateInvoiceAsync(paymentVm.BookingId);

            return true;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            if (payment.Amount <= 0)
            {
                return false;
            }

            Payment? existingPayment =
                await _paymentRepository.GetByIdAsync(payment.PaymentId);

            if (existingPayment == null)
            {
                return false;
            }

            int oldBookingId = existingPayment.BookingId;

            Booking? booking =
                await _bookingRepository.GetByIdAsync(payment.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments =
                await _paymentRepository.GetAllAsync();

            decimal otherPaymentsTotal = payments
                .Where(p =>
                    p.BookingId == payment.BookingId &&
                    p.PaymentId != payment.PaymentId)
                .Sum(p => p.Amount);

            decimal newTotalPaid =
                otherPaymentsTotal + payment.Amount;

            if (newTotalPaid > booking.TotalPrice)
            {
                return false;
            }

            existingPayment.BookingId = payment.BookingId;
            existingPayment.Amount = payment.Amount;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.PaymentDate = payment.PaymentDate;
            existingPayment.TransactionId = payment.TransactionId;
            existingPayment.Notes = payment.Notes;

            _paymentRepository.Update(existingPayment);

            int rowsAffected =
                await _paymentRepository.SaveChangesAsync();

            if (rowsAffected <= 0)
            {
                return false;
            }

            await RecalculateInvoiceAsync(oldBookingId);

            if (oldBookingId != payment.BookingId)
            {
                await RecalculateInvoiceAsync(payment.BookingId);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Payment? payment =
                await _paymentRepository.GetByIdAsync(id);

            if (payment == null)
            {
                return false;
            }

            int bookingId = payment.BookingId;

            bool result =
                await _paymentRepository.DeleteAsync(id);

            if (!result)
            {
                return false;
            }

            await RecalculateInvoiceAsync(bookingId);

            return true;
        }

        private async Task RecalculateInvoiceAsync(int bookingId)
        {
            Booking? booking =
                await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return;
            }

            IEnumerable<Invoice> invoices =
                await _invoiceRepository.GetAllAsync();

            Invoice? invoice = invoices.FirstOrDefault(i => i.BookingId == bookingId);

            IEnumerable<Payment> payments =
                await _paymentRepository.GetAllAsync();

            decimal paidAmount = payments
                .Where(p => p.BookingId == bookingId)
                .Sum(p => p.Amount);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    BookingId = bookingId,
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{bookingId:D4}",
                    IssueDate = DateTime.Now,
                    TotalAmount = booking.TotalPrice,
                    PaidAmount = paidAmount,
                    RemainingAmount = Math.Max(0, booking.TotalPrice - paidAmount),
                    CreatedAt = DateTime.Now
                };
                await _invoiceRepository.AddAsync(invoice);
            }
            else
            {
                invoice.TotalAmount = booking.TotalPrice;
                invoice.PaidAmount = paidAmount;
                invoice.RemainingAmount = Math.Max(0, booking.TotalPrice - paidAmount);
                _invoiceRepository.Update(invoice);
            }

            await _invoiceRepository.SaveChangesAsync();
        }
    }
}