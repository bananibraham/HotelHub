using BLLayer1.ViewModel;
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.Interfaces
{
    public interface IPaymentBL
    {
        Task<IEnumerable<Payment>> GetAllAsync();

        Task<Payment?> GetByIdAsync(int id);

        Task<bool> CreateAsync(PaymentCreateVm paymentVm);

        Task<bool> UpdateAsync(Payment payment);

        Task<bool> DeleteAsync(int id);
    }
}
