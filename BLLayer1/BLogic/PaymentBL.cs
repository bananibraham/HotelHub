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

        public PaymentBL(
            IBasicOperation<Payment> paymentRepository,
            IBasicOperation<Booking> bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _paymentRepository.GetAllAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(PaymentCreateVm paymentVm)
        {
            if (paymentVm.Amount <= 0)
            {
                return false;
            }

            Booking? booking = await _bookingRepository.GetByIdAsync(paymentVm.BookingId);
            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments = await _paymentRepository.GetAllAsync();

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
            int rowsAffected = await _paymentRepository.SaveChangesAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(Payment payment)
        {
            if (payment.Amount <= 0)
            {
                return false;
            }

            Payment? existingPayment = await _paymentRepository.GetByIdAsync(payment.PaymentId);
            if (existingPayment == null)
            {
                return false;
            }

            Booking? booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments = await _paymentRepository.GetAllAsync();

            decimal otherPaymentsTotal = payments
                .Where(p =>
                    p.BookingId == payment.BookingId &&
                    p.PaymentId != payment.PaymentId)
                .Sum(p => p.Amount);

            decimal newTotalPaid = otherPaymentsTotal + payment.Amount;

            if (newTotalPaid > booking.TotalPrice)
            {
                return false;
            }

            _paymentRepository.Update(payment);
            int rowsAffected = await _paymentRepository.SaveChangesAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Payment? payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
            {
                return false;
            }

            return await _paymentRepository.DeleteAsync(id);
        }
    }
}