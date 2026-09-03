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

        public async Task<IEnumerable<Booking>> GetBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<bool> CreateAsync(InvoiceCreateVm invoiceVm)
        {
            Booking? booking =
                await _bookingRepository.GetByIdAsync(invoiceVm.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Invoice> invoices =
                await _invoiceRepository.GetAllAsync();

            bool invoiceExists =
                invoices.Any(i =>
                    i.BookingId == invoiceVm.BookingId);

            if (invoiceExists)
            {
                return false;
            }

            IEnumerable<Payment> payments =
                await _paymentRepository.GetAllAsync();

            decimal paidAmount = payments
                .Where(p =>
                    p.BookingId == invoiceVm.BookingId)
                .Sum(p => p.Amount);

            decimal totalAmount = booking.TotalPrice;

            decimal remainingAmount =
                totalAmount - paidAmount;

            if (remainingAmount < 0)
            {
                return false;
            }

            Invoice invoice = new Invoice
            {
                BookingId = invoiceVm.BookingId,
                InvoiceNumber = invoiceVm.InvoiceNumber.Trim(),
                IssueDate = invoiceVm.IssueDate,

                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                RemainingAmount = remainingAmount,

                CreatedAt = DateTime.Now
            };

            await _invoiceRepository.AddAsync(invoice);

            // ✅ FIXED: SaveChangesAsync returns Task, not int
            await _invoiceRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            Invoice? existingInvoice =
                await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);

            if (existingInvoice == null)
            {
                return false;
            }

            Booking? booking =
                await _bookingRepository.GetByIdAsync(invoice.BookingId);

            if (booking == null)
            {
                return false;
            }

            IEnumerable<Payment> payments =
                await _paymentRepository.GetAllAsync();

            decimal paidAmount = payments
                .Where(p =>
                    p.BookingId == invoice.BookingId)
                .Sum(p => p.Amount);

            decimal totalAmount = booking.TotalPrice;

            decimal remainingAmount =
                totalAmount - paidAmount;

            if (remainingAmount < 0)
            {
                return false;
            }

            existingInvoice.BookingId = invoice.BookingId;
            existingInvoice.InvoiceNumber =
                invoice.InvoiceNumber.Trim();
            existingInvoice.IssueDate = invoice.IssueDate;

            existingInvoice.TotalAmount = totalAmount;
            existingInvoice.PaidAmount = paidAmount;
            existingInvoice.RemainingAmount = remainingAmount;

            _invoiceRepository.Update(existingInvoice);

            // ✅ FIXED: SaveChangesAsync returns Task, not int
            await _invoiceRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Invoice? invoice =
                await _invoiceRepository.GetByIdAsync(id);

            if (invoice == null)
            {
                return false;
            }

            // ✅ FIXED: DeleteAsync returns Task, not Task<bool>
            await _invoiceRepository.DeleteAsync(id);
            await _invoiceRepository.SaveChangesAsync();
            
            return true;
        }
    }
}