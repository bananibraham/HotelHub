using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.BLogic
{
    public class InvoiceBL : IInvoiceBL
    {
        private readonly IBasicOperation<Invoice> _invoiceRepository;
        private readonly IBasicOperation<Booking> _bookingRepository;
        private readonly IBasicOperation<Payment> _paymentRepository;

        public InvoiceBL(
            IBasicOperation<Invoice> invoiceRepository,
            IBasicOperation<Booking> bookingRepository,
            IBasicOperation<Payment> paymentRepository)
        {
            _invoiceRepository = invoiceRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await _invoiceRepository.GetAllAsync();
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _invoiceRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(InvoiceCreateVm invoiceVm)
        {
            Booking? booking = await _bookingRepository.GetByIdAsync(invoiceVm.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Invoice> invoices = await _invoiceRepository.GetAllAsync();

            bool invoiceExists = invoices.Any(i => i.BookingId == invoiceVm.BookingId);

            if (invoiceExists)
            {
                return false;
            }

            IEnumerable<Payment> payments = await _paymentRepository.GetAllAsync();

            decimal paidAmount = payments
                .Where(p => p.BookingId == invoiceVm.BookingId)
                .Sum(p => p.Amount);

            decimal totalAmount = booking.TotalPrice;
            decimal remainingAmount = totalAmount - paidAmount;

            if (remainingAmount < 0)
            {
                return false;
            }

            Invoice invoice = new Invoice
            {
                BookingId = invoiceVm.BookingId,
                InvoiceNumber = invoiceVm.InvoiceNumber,
                IssueDate = invoiceVm.IssueDate,

                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = remainingAmount,

                CreatedAt = DateTime.Now
            };

            await _invoiceRepository.AddAsync(invoice);
            int rowsAffected = await _invoiceRepository.SaveChangesAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            Invoice? existingInvoice = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);

            if (existingInvoice == null)
            {
                return false;
            }

            Booking? booking = await _bookingRepository.GetByIdAsync(invoice.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments = await _paymentRepository.GetAllAsync();

            decimal paidAmount = payments
                .Where(p => p.BookingId == invoice.BookingId)
                .Sum(p => p.Amount);

            invoice.TotalAmount = booking.TotalPrice;
            invoice.PaidAmount = paidAmount;
            invoice.RemainingAmount = invoice.TotalAmount - invoice.PaidAmount;

            if (invoice.RemainingAmount < 0)
            {
                return false;
            }

            _invoiceRepository.Update(invoice);
            int rowsAffected = await _invoiceRepository.SaveChangesAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Invoice? invoice = await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return false;
            }

            return await _invoiceRepository.DeleteAsync(id);
        }
    }
}
