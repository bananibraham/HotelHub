using DataAccessLayer.Models;
using BLLayer1.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.Interfaces
{
    public interface IInvoiceBL
    {
        Task<IEnumerable<Invoice>> GetAllAsync();

        Task<Invoice?> GetByIdAsync(int id);

        Task<IEnumerable<Booking>> GetBookingsAsync();

        Task<bool> CreateAsync(InvoiceCreateVm invoiceVm);

        Task<bool> UpdateAsync(Invoice invoice);

        Task<bool> DeleteAsync(int id);
    }
}
